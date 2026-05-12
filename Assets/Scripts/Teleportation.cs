using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Teleportation : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private string locationID;

    public async void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeOut();

        player.transform.position = teleportTarget.position;

        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        CinemachinePositionComposer composer = vcam?.GetComponent<CinemachinePositionComposer>();

        if (composer != null)
        {
            Vector3 oldDamping = new Vector3 (composer.Damping.x, composer.Damping.y,composer.Damping.z);
            composer.Damping = Vector3.zero;
            await ScreenFader.Instance.FadeIn();
            composer.Damping = oldDamping;
        }
        else
        {
            if (ScreenFader.Instance != null) await ScreenFader.Instance.FadeIn();
        }

        if (!string.IsNullOrEmpty(locationID))
        {
            QuestManager.Instance.RegisterLocation(locationID);
        }
    }
}
