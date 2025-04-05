﻿using AutoMapper;
using LayeredAppTemplate.Application.DTOs.User;
using LayeredAppTemplate.Domain.Entities;

namespace LayeredAppTemplate.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();

            // Yeni entity'leri buraya eklersin:
            // CreateMap<Product, ProductDto>().ReverseMap();
        }
    }
}
