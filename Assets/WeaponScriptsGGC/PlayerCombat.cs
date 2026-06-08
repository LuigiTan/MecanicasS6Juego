using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;
    public BaseWeapon[] weapons; // Debemos ponerlas en orden: 0=Melee, 1=Ranged, 2=AoE

    public Image[] images; 

    private int currentWeaponIndex = 0;
    private MeleeWeapon meleeWeapon;
    public TextMeshProUGUI currentWeaponText;

    public Color originalColor = Color.white;
    public Color unusedColor = Color.gray;

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
            images[i].color = unusedColor;
            images[i].gameObject.SetActive(false);
            Debug.Log("WEAPON " + 1);
        }

        currentWeaponIndex = index;
        if (currentWeaponIndex == 0) 
        {
            currentWeaponText.text = "2       -       3";
            for (int i = 0; i < weapons.Length; i++)
            {
                if (images[i].IsActive()) images[i].gameObject.SetActive(false);
                if (images[i].color == Color.white) images[i].color = unusedColor;
                Debug.Log("WEAPON " + 2);
            }

            images[0].gameObject.SetActive(true);
            images[4].gameObject.SetActive(true);
            images[5].gameObject.SetActive(true);

            images[0].color = originalColor;
            images[4].color = unusedColor;
            images[5].color = unusedColor;
        }
        else if (currentWeaponIndex == 1)
        {
            currentWeaponText.text = "1       -       3";
            for (int i = 0; i < weapons.Length; i++)
            {
                if (images[i].IsActive()) images[i].gameObject.SetActive(false);
                if (images[i].color == Color.white) images[i].color = unusedColor;
                Debug.Log("WEAPON " + 3);
            }

            images[3].gameObject.SetActive(true);
            images[1].gameObject.SetActive(true);
            images[5].gameObject.SetActive(true);

            images[3].color = unusedColor;
            images[1].color = originalColor;
            images[5].color = unusedColor;
        }
        else if (currentWeaponIndex == 2)
        {
            currentWeaponText.text = "1       -       2";
            for (int i = 0; i < weapons.Length; i++)
            {
                if (images[i].IsActive()) images[i].gameObject.SetActive(false);
                if (images[i].color == Color.white) images[i].color = unusedColor;
            }

            images[3].gameObject.SetActive(true);
            images[6].gameObject.SetActive(true);
            images[2].gameObject.SetActive(true);

            images[3].color = unusedColor;
            images[6].color = unusedColor;
            images[2].color = originalColor;
        }

        if (weapons[index].weaponType == WeaponType.Melee)
            meleeWeapon = weapons[index] as MeleeWeapon;
        else
            meleeWeapon = null;
    }
}

