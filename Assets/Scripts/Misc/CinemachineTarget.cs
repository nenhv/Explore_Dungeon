
using UnityEngine;


[RequireComponent(typeof(Unity.Cinemachine.CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    private Unity.Cinemachine.CinemachineTargetGroup cinemachineTargetGroup;

    [SerializeField] private Transform cursorTarget;

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<Unity.Cinemachine.CinemachineTargetGroup>();
    }

    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    private void SetCinemachineTargetGroup()
    {
        Transform playerTarget =
            GameManager.Instance.GetPlayer().transform;

        // Clear existing targets to avoid duplicates.
        cinemachineTargetGroup.m_Targets =
            new Unity.Cinemachine.CinemachineTargetGroup.Target[0];

        // Player target
        cinemachineTargetGroup.AddMember(
            playerTarget,
            1f,
            2.5f
        );

        // Mouse cursor target
        cinemachineTargetGroup.AddMember(
            cursorTarget,
            0.55f,
            2f
        );
    }

    private void Update()
    {
        cursorTarget.position =
            HelperUtilities.GetMouseWorldPosition();
    }
}
