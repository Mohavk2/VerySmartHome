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

        public GroupDTO ToGroupDTO(BulbGroup group)
        {
            Dictionary<string, BulbDTO> temp = new Dictionary<string, BulbDTO>();

            foreach (ColorBulbProxy bulb in group)
            {
                temp.Add(bulb.Id, ToBulbDto(bulb));
            }

            return new GroupDTO
            {
                Id = group.Id,
                Name = group.Name,
                Bulbs = temp 
            };
        }
        public List<BulbDTO> ToListBulbDTO(List<ColorBulbProxy> group)
        {
            List<BulbDTO> temp = new List<BulbDTO>();

            foreach (ColorBulbProxy bulb in group)
            {
                temp.Add(ToBulbDto(bulb));
            }

            return temp;
        }
        public List<GroupDTO> ToListGroupDTO(List<BulbGroup> groups)
        {
            List<GroupDTO> temp = new List<GroupDTO>();

            foreach(var group in groups)
            {
                temp.Add(ToGroupDTO(group));
            }
            return temp;
        }
    }
}
