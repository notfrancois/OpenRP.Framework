﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRP.Framework.Features.Inventories.Services
{
    public interface IInventoryItemManager
    {
        void ProcessChanges();
        int LoadInventoryItems();
    }
}
