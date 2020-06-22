using SmartBulbColor.Domain;
using SmartBulbColor.Domain.Entities;
using SmartBulbColor.PluginApp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartBulbColor.PluginApp
{
    internal class DtoMapper
    {
        public BulbDTO ToBulbDto(ColorBulbProxy bulb)
        {
            BulbDTO bulbDTO = new BulbDTO
            {
                Id = bulb.Id,
                Name = bulb.Name,
                Model = bulb.Model,
                FwVer = bulb.FwVer,
                Ip = bulb.Ip,
                BelongsToGroup = bulb.BelongsToGroup,
                IsOnline = bulb.IsOnline,
                IsPowered = bulb.IsPowered,
                IsMusicModeOn = bulb.IsMusicModeOn,
                ColorMode = bulb.ColorMode,
                Brightness = bulb.Brightness,
                ColorTemperature = bulb.ColorTemperature,
                Rgb = bulb.Rgb,
                Hue = bulb.Hue,
                Saturation = bulb.Saturation,
                Flowing = bulb.Flowing,
                Delayoff = bulb.Delayoff
            };
            return bulbDTO;
        }
        public List<BulbDTO> ToListBulbDTO(IEnumerable<ColorBulbProxy> group)
        {
            List<BulbDTO> tempGroup = new List<BulbDTO>();

            foreach (ColorBulbProxy bulb in group)
            {
                BulbDTO tempBulb = ToBulbDto(bulb);
                tempGroup.Add(tempBulb);
            }
            return tempGroup;
        }
        public GroupDTO ToGroupDTO(BulbGroup group)
        {
            List<BulbDTO> temp = new List<BulbDTO>();

            foreach (ColorBulbProxy bulb in group)
            {
                temp.Add(ToBulbDto(bulb));
            }

            return new GroupDTO
            {
                Id = group.Id,
                Name = group.Name,
                Bulbs = temp 
            };
        }
        public List<GroupDTO> ToListGroupDTO(IEnumerable<BulbGroup> groups)
        {
            List<GroupDTO> tempGroups = new List<GroupDTO>();

            foreach(var group in groups)
            {
                GroupDTO tempGroup = ToGroupDTO(group);
                tempGroups.Add(tempGroup);
            }
            return tempGroups;
        }
    }
}
