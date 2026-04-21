using UnityEngine;
using UnityEngine.InputSystem;

internal static class PlayerHudTargetResolver
{
    public static GameObject ResolveLocalPlayerRoot()
    {
        PlayerShooting playerShooting = Object.FindFirstObjectByType<PlayerShooting>(FindObjectsInactive.Exclude);

        if (playerShooting != null)
        {
            return playerShooting.gameObject;
        }

        AmmoSystem ammoSystem = Object.FindFirstObjectByType<AmmoSystem>(FindObjectsInactive.Exclude);

        if (ammoSystem != null)
        {
            return ammoSystem.gameObject;
        }

        PlayerInput playerInput = Object.FindFirstObjectByType<PlayerInput>(FindObjectsInactive.Exclude);

        return playerInput != null ? playerInput.gameObject : null;
    }
}
