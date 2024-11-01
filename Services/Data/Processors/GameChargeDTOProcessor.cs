﻿using WebAPI.Models;

namespace WebAPI.Services.Data.Processors;

/// <summary>
/// Transforms a <see cref="GameCharge"/> into a <see cref="GameChargeDTO"/>.
/// This does not apply any business logic.
/// </summary>
public class GameChargeDTOProcessor : IDataProcessor<GameCharge?,GameChargeDTO?> {
    public GameChargeDTO? Process(GameCharge? input) {
        GameChargeDTO? result = null;

        if (input is not null) {
            result = new GameChargeDTO() {
                Number = input.Id,
                Customer = input.CustomerId,
                Game = input.GameId,
                GameName = input.GameName,
                ChargeDate = input.ChargeDate,
                Cost = input.Cost,
            };
        }

        return result;
    }
}

public static class GameChargeDTOProcessorExtensions {
    public static IServiceCollection AddGameChargeProcessor(this IServiceCollection collection) {
        collection.AddSingleton<IDataProcessor<GameCharge?, GameChargeDTO?>, GameChargeDTOProcessor>();
        return collection;
    }
}
