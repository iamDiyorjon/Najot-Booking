﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NajotBooking.Api.Models.Seats;
using NajotBooking.Api.Models.Seats.Exceptions;
using Xunit;

namespace NajotBooking.Api.Tests.Unit.Services.Foundations.Seats
{
    public partial class SeatServiceTests
    {
        [Fact]
        public async Task ShouldThrowValidationExceptionOnRemoveIfIdIsInvalidAndLogItAsync()
        {
            // given
            Guid invalidSeatId = Guid.Empty;

            var invalidSeatException =
                new InvalidSeatException();

            invalidSeatException.AddData(
                key: nameof(Seat.Id),
                values: "Id is required");

            var expectedSeatValidationException =
                new SeatValidationException(invalidSeatException);

            // when
            ValueTask<Seat> removeSeatByIdTask =
                this.seatService.RemoveSeatByIdAsync(invalidSeatId);

            SeatValidationException actualSeatValidationException =
                await Assert.ThrowsAsync<SeatValidationException>(removeSeatByIdTask.AsTask);

            //then
            actualSeatValidationException.Should().BeEquivalentTo(expectedSeatValidationException);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedSeatValidationException))), Times.Once);

            this.storageBrokerMock.Verify(broker =>
                broker.DeleteSeatAsync(It.IsAny<Seat>()), Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.storageBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowNotFoundExceptionOnRemoveIfSeatIsNotFoundAndLogItAsync()
        {
            //given
            Guid randomSeatId = Guid.NewGuid();
            Guid inputSeatId = randomSeatId;
            Seat noSeat = null;

            var notFoundSeatException =
                new NotFoundSeatException(inputSeatId);

            var expectedSeatValidationException =
                new SeatValidationException(notFoundSeatException);

            this.storageBrokerMock.Setup(broker =>
                broker.SelectSeatByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(noSeat);

            //when
            ValueTask<Seat> removeSeatByIdTask =
                this.seatService.RemoveSeatByIdAsync(inputSeatId);

            SeatValidationException actualSeatValidationException =
                await Assert.ThrowsAsync<SeatValidationException>(
                    removeSeatByIdTask.AsTask);

            //then
            actualSeatValidationException.Should()
                .BeEquivalentTo(expectedSeatValidationException);

            this.storageBrokerMock.Verify(broker =>
                broker.SelectSeatByIdAsync(It.IsAny<Guid>()), Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogError(It.Is(SameExceptionAs(
                    expectedSeatValidationException))), Times.Once);

            this.storageBrokerMock.Verify(broker =>
              broker.DeleteSeatAsync(It.IsAny<Seat>()), Times.Never);

            this.storageBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
