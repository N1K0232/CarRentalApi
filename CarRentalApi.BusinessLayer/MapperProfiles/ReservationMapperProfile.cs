using AutoMapper;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.MapperProfiles;

public class ReservationMapperProfile : Profile
{
	public ReservationMapperProfile()
	{
		CreateMap<Entities.Reservation, Reservation>();
		CreateMap<SaveReservationRequest, Entities.Reservation>();
	}
}