using AutoMapper;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.MapperProfiles;

public class VehicleMapperProfile : Profile
{
	public VehicleMapperProfile()
	{
		CreateMap<Entities.Vehicle, Vehicle>();
		CreateMap<SaveVehicleRequest, Entities.Vehicle>();
	}
}