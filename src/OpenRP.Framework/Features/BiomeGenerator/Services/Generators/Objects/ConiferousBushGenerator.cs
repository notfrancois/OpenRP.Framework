﻿using OpenRP.Framework.Features.BiomeGenerator.Entities;
using SampSharp.Entities.SAMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRP.Framework.Features.BiomeGenerator.Services.Generators.Objects
{
    public class ConiferousBushGenerator : IBiomeObjectGenerator
    {
        public string ObjectType => "ConiferousBush";

        public BiomeObject Generate(Vector2 virtualPosition, Vector3 gamePosition, Vector3 gameRotation, Vector3 defaultRotation, Color outputColor)
        {
            int[] obj_arr_bush = { 811, 804, 824 };

            int modelId = obj_arr_bush[Random.Shared.Next(obj_arr_bush.Length)];

            BiomeObject bushObject = new BiomeObject(
                obj_arr_bush[Random.Shared.Next(obj_arr_bush.Length)],
                virtualPosition,
                new Vector3(gamePosition.XY, gamePosition.Z),
                gameRotation,
                outputColor
            );

            return bushObject;
        }
    }
}
