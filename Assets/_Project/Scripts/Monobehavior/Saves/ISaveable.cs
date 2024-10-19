using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    void ReloadFromSafe(DateTime saveDateStamp);
    void SaveData(DateTime saveDateStamp);
}
