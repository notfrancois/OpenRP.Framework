﻿using OpenRP.Framework.Features.BiomeGenerator.Entities;
using SampSharp.Entities.SAMP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRP.Framework.Features.BiomeGenerator.Services.Generators.Objects
{
    public class DryBushGenerator : IBiomeObjectGenerator
    {
        public string ObjectType => "DryBush";

        public BiomeObject Generate(Vector2 virtualPosition, Vector3 gamePosition, Vector3 gameRotation, Vector3 defaultRotation, Color outputColor)
        {
            int[] obj_arr_dry_bushes = { 761, 692 };

            int modelId = obj_arr_dry_bushes[Random.Shared.Next(obj_arr_dry_bushes.Length)];

            BiomeObject dryBushObject = new BiomeObject(
                obj_arr_dry_bushes[Random.Shared.Next(obj_arr_dry_bushes.Length)],
                virtualPosition,
                gamePosition,
                gameRotation,
                outputColor
            );

            return dryBushObject;
        }
    }
}
