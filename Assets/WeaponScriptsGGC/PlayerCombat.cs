using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public BaseWeapon[] weapons; // Debemos ponerlas en orden: 0=Melee, 1=Ranged, 2=AoE

    private int currentWeaponIndex = 0;
    private MeleeWeapon meleeWeapon;

    void Start()
    {
        SelectWeapon(0);
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleAttackInput();
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2);
    }

    void HandleAttackInput()
    {
        BaseWeapon currentWeapon = weapons[currentWeaponIndex];

        if (currentWeapon.weaponType == WeaponType.Melee)
        {
            HandleMeleeInput();
        }
        else
        {
            if (Input.GetButton("Fire1")) // Click izquierdo
                currentWeapon.TryAttack();
        }
    }

    void HandleMeleeInput()
    {
        if (meleeWeapon == null) return;

        if (Input.GetButtonDown("Fire2")) // Clic derecho presionado
        {
            meleeWeapon.BeginPreparation();
        }
        if (Input.GetButtonUp("Fire2")) // Clic derecho soltado
        {
            meleeWeapon.CancelPreparation();
        }

        if (Input.GetButtonDown("Fire1")) // Clic izquierdo presionado
        {
            meleeWeapon.ConfirmReady();
            meleeWeapon.TryAttack();
        }
    }

    void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }

        currentWeaponIndex = index;

        if (weapons[index].weaponType == WeaponType.Melee)
            meleeWeapon = weapons[index] as MeleeWeapon;
        else
            meleeWeapon = null;
    }
}

