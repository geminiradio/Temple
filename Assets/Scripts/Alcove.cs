using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Alcove : TempleWallBlock {

    public ContainerConfiguration[] configurations;
    public float[] config_randomWeights;

    private ContainerConfiguration currentConfig;


    protected override void Start ()
    {
        base.Start();

        CodeTools.ValidateWeightedRandomArray(config_randomWeights, configurations.Length);

    }

    protected override void StartEmerging()
    {
        base.StartEmerging();

        // pick one of the configurations at random
        currentConfig = configurations[CodeTools.WeightedRandomSelection(config_randomWeights)];
        currentConfig.gameObject.SetActive(true);
        currentConfig.InitializeContainerConfiguration();
    }


}
