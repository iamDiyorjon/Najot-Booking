﻿// ---------------------------------------------------------------
// Copyright (c) Coalition Of The THE STANDART SHARPISTS
// Free To Use To Book Places In Coworking Zones
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using NajotBooking.Api.Models.Users;
using NajotBooking.Api.Models.Users.Exceptions;
using Xunit;

namespace NajotBooking.Api.Tests.Unit.Services.Foundations.Users
{
    public partial class UserServiceTests
    {
        [Fact]
        public async Task ShouldThrowCriticalDependencyExceptionOnModifyIfSqlErrorOccursAndLogItAsync()
        {
            // given
            DateTimeOffset someDateTime = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser(someDateTime);
            User someUser = randomUser;
            Guid userId = someUser.Id;
            SqlException sqlException = GetSqlException();

            var failedUserStorageException =
                new FailedUserStorageException(sqlException);

            var expectedUserDependencyException =
                new UserDependencyException(failedUserStorageException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectUserByIdAsync(userId))
                    .Throws(sqlException);

            // when
            ValueTask<User> modifyUserTask =
                this.userService.ModifyUserAsync(someUser);

            UserDependencyException actualUserDependencyException =
                await Assert.ThrowsAsync<UserDependencyException>(
                     modifyUserTask.AsTask);

            // then
            actualUserDependencyException.Should().BeEquivalentTo(
                expectedUserDependencyException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCritical(It.Is(SameExceptionAs(
                    expectedUserDependencyException))), Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectUserByIdAsync(userId), Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.UpdateUserAsync(someUser), Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnModifyIfDatabaseUpdateExceptionOccursAndLogItAsync()
        {
            // given
            int minutesInPast = GetRandomNegativeNumber();
            DateTimeOffset randomDateTime = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser(randomDateTime);
            User someUser = randomUser;
            Guid userId = someUser.Id;
            var databaseUpdateException = new DbUpdateException();

            var failedUserException =
                new FailedUserStorageException(databaseUpdateException);

            var expectedUserDependencyException =
                new UserDependencyException(failedUserException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectUserByIdAsync(userId))
                    .ThrowsAsync(databaseUpdateException);

            // when
            ValueTask<User> modifyUserTask =
                this.userService.ModifyUserAsync(someUser);

            UserDependencyException actualUserDependencyException =
                 await Assert.ThrowsAsync<UserDependencyException>(
                     modifyUserTask.AsTask);

            // then
            actualUserDependencyException.Should().BeEquivalentTo(
                expectedUserDependencyException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectUserByIdAsync(userId), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedUserDependencyException))), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDatabaseUpdateConcurrencyErrorOccursAndLogItAsync()
        {
            // given
            int minutesInPast = GetRandomNegativeNumber();
            DateTimeOffset randomDateTime = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser(randomDateTime);
            User someUser = randomUser;
            Guid userId = someUser.Id;
            var databaseUpdateConcurrencyException = new DbUpdateConcurrencyException();

            var lockedUserException =
                new LockedUserException(databaseUpdateConcurrencyException);

            var expectedUserDependencyValidationException =
                new UserDependencyValidationException(lockedUserException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectUserByIdAsync(userId))
                    .ThrowsAsync(databaseUpdateConcurrencyException);

            // when
            ValueTask<User> modifyUserTask =
                this.userService.ModifyUserAsync(someUser);

            UserDependencyValidationException actualUserDependencyValidationException =
                await Assert.ThrowsAsync<UserDependencyValidationException>(
                    modifyUserTask.AsTask);

            // then
            actualUserDependencyValidationException.Should().BeEquivalentTo(
                expectedUserDependencyValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectUserByIdAsync(userId), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedUserDependencyValidationException))), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnModifyIfDatabaseUpdateErrorOccursAndLogItAsync()
        {
            // given
            int minuteInPast = GetRandomNegativeNumber();
            DateTimeOffset randomDateTime = GetRandomDateTimeOffset();
            User randomUser = CreateRandomUser(randomDateTime);
            User someUser = randomUser;
            var serviceException = new Exception();

            var failedUserException =
                new FailedUserServiceException(serviceException);

            var expectedUserServiceException =
                new UserServiceException(failedUserException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectUserByIdAsync(someUser.Id))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<User> modifyUserTask =
                this.userService.ModifyUserAsync(someUser);

            UserServiceException actualUserServiceException =
                await Assert.ThrowsAsync<UserServiceException>(
                    modifyUserTask.AsTask);

            // then
            actualUserServiceException.Should().BeEquivalentTo(
                expectedUserServiceException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectUserByIdAsync(someUser.Id), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedUserServiceException))), Times.Once);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}