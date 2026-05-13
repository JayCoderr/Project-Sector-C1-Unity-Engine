using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MFPS.Internal
{
    public class bl_AnimatorLayerWeightEvent : StateMachineBehaviour
    {
        public int LayerIndex = 0;
        public float onEnterWeight = 0;
        public float onExitWeight = 1;

        CancellationTokenSource cts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
            cts = new CancellationTokenSource();
            LerpWeightAsync(animator, onExitWeight, onEnterWeight, cts.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
            cts = new CancellationTokenSource();
            LerpWeightAsync(animator, onEnterWeight, onExitWeight, cts.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async void LerpWeightAsync(Animator animator, float from, float to, CancellationToken token)
        {
            float d = 0;
            while (d < 1)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                d += Time.deltaTime * 3;
                if (animator != null) animator.SetLayerWeight(LayerIndex, Mathf.Lerp(from, to, d));
                await Task.Yield(); // This will yield execution until the next frame.
            }
        }
    }
}