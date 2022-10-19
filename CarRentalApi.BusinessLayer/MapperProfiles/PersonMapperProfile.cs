using AutoMapper;
using CarRentalApi.Shared.Models;
using CarRentalApi.Shared.Requests;
using Entities = CarRentalApi.DataAccessLayer.Entities;

namespace CarRentalApi.BusinessLayer.MapperProfiles;

public class PersonMapperProfile : Profile
{
	public PersonMapperProfile()
	{
		CreateMap<Entities.Person, Person>();
		CreateMap<SavePersonRequest, Entities.Person>();
	}
}