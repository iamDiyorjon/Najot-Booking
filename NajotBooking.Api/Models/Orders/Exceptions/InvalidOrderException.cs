﻿// ---------------------------------------------------------------
// Copyright (c) Coalition of the THE STANDART SHARPISTS
// Free To Use to Book Places in Coworking Zones
// ---------------------------------------------------------------

using Xeptions;

namespace NajotBooking.Api.Models.Orders.Exceptions
{
    public class InvalidOrderException : Xeption
    {
        public InvalidOrderException()
            : base(message: "Order is invalid.")
        { }
    }
}
