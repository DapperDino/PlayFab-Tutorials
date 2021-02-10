using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;
using TMPro;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private TMP_Text scoreText = default;

        public float speed = 30;
        public Rigidbody2D rigidbody2d;

        [SyncVar(hook = nameof(HandleScoreUpdated))]
        private int score;

        private string playFabId;

        public override void OnStartAuthority()
        {
            CmdSetSessionTicket(PlayFabLogin.SessionTicket);
        }

        [Command]
        private void CmdSetSessionTicket(string SessionTicket)
        {
            PlayFabServerAPI.AuthenticateSessionTicket(new AuthenticateSessionTicketRequest
            {
                SessionTicket = SessionTicket
            }, result =>
            {
                playFabId = result.UserInfo.PlayFabId;
                GetScore();
            }, error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        [Server]
        private void GetScore()
        {
            PlayFabServerAPI.GetUserData(new GetUserDataRequest
            {
                PlayFabId = playFabId
            }, result =>
            {
                if (result.Data != null && result.Data.ContainsKey("Score"))
                {
                    score = int.Parse(result.Data["Score"].Value);
                }
            }, error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        [Server]
        public void IncrementScore()
        {
            score++;

            PlayFabServerAPI.UpdateUserData(new UpdateUserDataRequest
            {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string>
                {
                    { "Score", score.ToString() },
                }
            }, result =>
            {
                Debug.Log("Updated Score");
            }, error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        private void HandleScoreUpdated(int oldScore, int newScore)
        {
            scoreText.text = newScore.ToString();
        }

        // need to use FixedUpdate for rigidbody
        void FixedUpdate()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
                rigidbody2d.velocity = new Vector2(0, Input.GetAxisRaw("Vertical")) * speed * Time.fixedDeltaTime;
        }
    }
}
