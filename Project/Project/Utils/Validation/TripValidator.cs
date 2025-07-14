using FluentValidation;
using Project.DTOs;

namespace Project.Utils.Validation
{
    public class TripValidator : AbstractValidator<TripDTO>
    {
        public TripValidator()
        {
            RuleFor(x => x.TrainId)
            .GreaterThan(0).WithMessage("TrainId phải > 0");

            RuleFor(x => x.RouteId)
                .GreaterThan(0).WithMessage("RouteId phải > 0");

            RuleFor(x => x.DepartureTime)
                .LessThan(x => x.ArrivalTime)
                .WithMessage("Giờ khởi hành phải trước giờ đến");

            RuleFor(x => x.DepartureStationId)
                .NotEqual(x => x.ArrivalStationId)
                .WithMessage("Ga đi và ga đến không được trùng nhau");
        }
    }
}
