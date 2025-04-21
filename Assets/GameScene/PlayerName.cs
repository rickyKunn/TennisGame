using Fusion;
using UnityEngine;

public class PlayerName : NetworkBehaviour
{
    // 16 文字まで同期。変化したら OnNickNameChanged() が呼ばれる
    [Networked, Capacity(16), OnChangedRender(nameof(OnNickNameChanged))]
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
