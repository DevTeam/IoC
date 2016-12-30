﻿namespace DevTeam.IoC.Contracts
{
    public interface IProvider<in TSTate1, in TSTate2, in TSTate3, in TSTate4, TContract>
    {
        bool TryGet(out TContract instance, TSTate1 state1, TSTate2 state2, TSTate3 state3, TSTate4 state4);
    }
}