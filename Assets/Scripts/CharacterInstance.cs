using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInstance : MonoBehaviour
{
    private WeaponManager weaponManager;

    //enum CharacterClass
    //{
    //    WARRIOR,
    //    MAGE,
    //    ARCHER,
    //    THEIF
    //}

    //struct CharacterData
    //{
    //    CharacterClass characterClass;
    //}

    private void Awake()
    {
        // switch (characterClass)
        weaponManager = GetComponent<WeaponManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
