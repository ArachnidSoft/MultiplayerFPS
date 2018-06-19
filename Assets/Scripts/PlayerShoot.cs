using UnityEngine.Networking;
using UnityEngine;

[RequireComponent (typeof (WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    
    private PlayerWeapon currentWeapon;


    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LayerMask mask;

    private WeaponManager weaponManager;
	// Use this for initialization
	void Start ()
    {
        if (cam == null)
        {
            Debug.LogError("PlayerShoor: No camera referenced.");
            this.enabled = false;
        }

        weaponManager = GetComponent<WeaponManager>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (currentWeapon.fireRate <= 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if(Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
            }
            else if(Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
	}



    //is called on the server when a player shoots
    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    //is callled on all clients when we need ot do a shoot effect
    [ClientRpc]
    void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    //Is called on the server when we hit something. Takes in the hit point and the normal on the surface
    [Command]
    void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }

    //is called on all clients. Here we spawn effects
    [ClientRpc]
    void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    private void Shoot()
    {
        if(!isLocalPlayer)
        {
            return;
        }
        //We are shooting / calling the on shoot method on the server
        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position,cam.transform.forward,out _hit, currentWeapon.range, mask))
        {
            if (_hit.collider.tag == PLAYER_TAG)
            {
                
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage);
            }

            //We hit something, call the on hit method on the server
            CmdOnHit(_hit.point, _hit.normal);
        }
    }

    [Command]
    void CmdPlayerShot(string _playerID, int damage)
    {
        Debug.Log(_playerID + " has been shot");

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(damage);

    }

}
