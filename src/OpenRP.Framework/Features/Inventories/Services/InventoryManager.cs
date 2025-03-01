﻿using Microsoft.Extensions.Logging;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database;
using OpenRP.Framework.Features.Items.Components;
using OpenRP.Framework.Features.Items.Entities;
using OpenRP.Framework.Features.Items.Services;
using SampSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRP.Framework.Features.Inventories.Entities;
using OpenRP.Framework.Features.Inventories.Components;
using Microsoft.EntityFrameworkCore;

namespace OpenRP.Framework.Features.Inventories.Services
{
    public class InventoryManager : IInventoryManager
    {
        private BaseDataContext _dataContext;
        private IEntityManager _entityManager;
        private ILogger<InventoryManager> _logger;
        private DateTime _lastUpdate;

        public InventoryManager(
            BaseDataContext dataContext,
            IEntityManager entityManager,
            ILogger<InventoryManager> logger
        )
        {
            _dataContext = dataContext;
            _entityManager = entityManager;
            _logger = logger;

            // Change last update
            _lastUpdate = DateTime.UtcNow;
        }

        public void ProcessChanges()
        {
            DateTime changesSince = _lastUpdate;
            _lastUpdate = DateTime.Now;

            int inventoriesAdded = LoadNewInventories(changesSince);
            int inventoriesUpdated = UpdateInventories(changesSince);
            int inventoriesSaved = SaveInventories();
            int inventoriesCreated = CreateInventories();
            int inventoriesDeleted = DeleteInventories();
        }

        public int LoadInventories()
        {
            _logger.LogInformation("Begin loading inventories from database.");
            List<InventoryModel> inventoryModels = _dataContext.Inventories
                .AsNoTracking()
                .ToList();

            int amountLoaded = 0;
            foreach (InventoryModel inventoryModel in inventoryModels)
            {
                EntityId inventoryEntityId = InventoryEntities.GetInventoryId((int)inventoryModel.Id);
                _entityManager.Create(inventoryEntityId);

                Inventory inventory = _entityManager.AddComponent<Inventory>(inventoryEntityId, inventoryModel);

                amountLoaded++;
            }

            _logger.LogInformation("Loaded {0} inventories.", amountLoaded);

            _logger.LogInformation("Finished loading inventories from database.");
            return amountLoaded;
        }

        private int LoadNewInventories(DateTime changesSince)
        {
            List<InventoryModel> inventoryModels = _dataContext.Inventories
                .Where(i => i.CreatedOn > changesSince)
                .AsNoTracking()
                .ToList();

            int amountLoaded = 0;
            foreach (InventoryModel inventoryModel in inventoryModels)
            {
                EntityId inventoryEntityId = InventoryEntities.GetInventoryId((int)inventoryModel.Id);
                _entityManager.Create(inventoryEntityId);

                Inventory inventory = _entityManager.AddComponent<Inventory>(inventoryEntityId, inventoryModel);

                amountLoaded++;
            }

            return amountLoaded;
        }

        private int UpdateInventories(DateTime changesSince)
        {
            List<InventoryModel> inventoryModels = _dataContext.Inventories
                .Where(i => i.UpdatedOn > changesSince)
                .AsNoTracking()
                .ToList();

            int amountLoaded = 0;
            foreach (InventoryModel inventoryModel in inventoryModels)
            {
                EntityId inventoryEntityId = InventoryEntities.GetInventoryId((int)inventoryModel.Id);

                _entityManager.Destroy(inventoryEntityId);
                Inventory inventory = _entityManager.AddComponent<Inventory>(inventoryEntityId, inventoryModel);

                amountLoaded++;
            }

            return amountLoaded;
        }

        private int SaveInventories()
        {
            int amountLoaded = 0;
            foreach (Inventory inventory in _entityManager.GetComponents<Inventory>())
            {
                if(inventory.HasChanges())
                {
                    InventoryModel? inventoryModel = _dataContext.Inventories.FirstOrDefault(i => i.Id == inventory.GetId());

                    if (inventoryModel != null)
                    {
                        inventoryModel.MaxWeight = inventory.GetMaxWeight();
                        if (_dataContext.SaveChanges() > 0)
                        {
                            inventory.ProcessChanges(false);
                            amountLoaded++;
                        }
                    }
                }
            }

            return amountLoaded;
        }

        private async Task<int> CreateInventories()
        {
            int amountCreated = 0;
            foreach (Inventory inventory in _entityManager.GetComponents<Inventory>())
            {
                if (inventory.GetId() == 0)
                {
                    InventoryModel inventoryModel = inventory.GetRawInventoryModel();

                    if (inventoryModel != null)
                    {
                        InventoryModel createdInventoryModel = _dataContext.Inventories.Update(inventoryModel).Entity;

                        if (await _dataContext.SaveChangesAsync() > 0)
                        {
                            inventory.ProcessChanges(false);

                            inventory.Destroy();

                            EntityId inventoryEntityId = InventoryEntities.GetInventoryId((int)createdInventoryModel.Id);

                            _entityManager.Create(inventoryEntityId);

                            Inventory newInventory = _entityManager.AddComponent<Inventory>(inventoryEntityId, createdInventoryModel, inventoryModel);

                            amountCreated++;
                        }
                    }
                }
            }

            InventoryNewEntities.ResetNewInventoryId();

            return amountCreated;
        }

        private async Task<int> DeleteInventories()
        {
            int amountLoaded = 0;
            foreach (Inventory inventory in _entityManager.GetComponents<Inventory>())
            {
                if (inventory.IsDeleted())
                {
                    InventoryModel inventoryModel = inventory.GetRawInventoryModel();

                    if (inventoryModel != null)
                    {
                        _dataContext.Inventories.Remove(inventoryModel);

                        if (await _dataContext.SaveChangesAsync() > 0)
                        {
                            inventory.ProcessChanges(false);
                            amountLoaded++;
                        }
                    }
                }
            }

            return amountLoaded;
        }
    }
}
