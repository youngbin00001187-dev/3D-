using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Encounter", menuName = "3DProject/Encounter")]
public class EncounterSO : ScriptableObject
{
    public string encounterName;
    public List<EnemyDataSO> enemies;
}
