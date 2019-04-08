using UnityEngine;

public interface IWeapon
{
    void Fire();
    void MakeKinematic(bool kine=true);

    Material GetMaterial();
    void SetMaterial(Material material);
    void Reload();

}