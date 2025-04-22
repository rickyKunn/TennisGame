using Fusion;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    // [Networked(OnChanged = nameof(OnNickNameChanged)), Capacity(16)]
    public string NickName { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority && string.IsNullOrEmpty(NickName))
            NickName = $"Player{Object.InputAuthority.PlayerId}";
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"NickName changed to: {NickName}", this);
    }
}
