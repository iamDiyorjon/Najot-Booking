﻿using Xeptions;

namespace NajotBooking.Api.Models.Seats.Exceptions
{
    public class NullSeatException : Xeption
    {
        public NullSeatException()
            : base(message: "Seat is null. ")
        { }
    }
}
