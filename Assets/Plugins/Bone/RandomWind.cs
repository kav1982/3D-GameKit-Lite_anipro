//
//RandomWind.cs for unity-chan!
//
//Original Script is here:
//ricopin / RandomWind.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
//修正2014/12/20
//风的方向变化/追加重力影响。


using UnityEngine;
using System.Collections;

namespace UnityChan
{
	public class RandomWind : MonoBehaviour
	{
		private SpringBone[] springBones;
		public bool isWindActive = false;

		private bool isMinus = false;				//风向反转用

		[Range(0.0f, 1.0f)]
		public float threshold = 0.5f;				//随机确定的阈值
		public float interval = 5.0f;				//随机判定的区间
		public float windPowerX = 1.0f;				//风的强度X轴向
		public float windPowerZ = 1.0f;				//风的强度Z轴向

		public float gravity = 0.98f;				//重力强度


		// Use this for initialization
		void Start ()
		{
            springBones = GetComponent<SpringManager> ().springBones;
			StartCoroutine ("RandomChange");
		}



		// Update is called once per frame
		void Update ()
		{
			if (isWindActive) 
			{
				Vector3 force = new Vector3(Mathf.PerlinNoise(Time.time, 0.0f) * windPowerX * 0.001f, 0, Mathf.PerlinNoise(Time.time, 0.0f) * windPowerZ * 0.001f);
				if(isMinus)
				{
					force *= -1;
				}

                force = transform.rotation * force;
                force += Vector3.up * (gravity * -0.001f);

                for (int i = 0; i < springBones.Length; i++) {
                    if (springBones[i] == null)
                        continue;

                    springBones [i].springForce = force;
                }

            }
		}
        /*
		void OnGUI ()
		{
			Rect rect1 = new Rect (10, Screen.height - 40, 400, 30);
			isWindActive = GUI.Toggle (rect1, isWindActive, "Random Wind");
		}
        */
		//随机判定函数
		IEnumerator RandomChange ()
		{
			//无限循环开始
			while (true) {
				//发生随机判定用种子
				float _seed = Random.Range (0.0f, 1.0f);

				if (_seed > threshold) {
					//_seed在threshold以上时，反转符号
					isMinus = true;
				}else{
					isMinus = false;
				}

				//间隔到下一个判定
				yield return new WaitForSeconds (interval);
			}
		}


	}
}