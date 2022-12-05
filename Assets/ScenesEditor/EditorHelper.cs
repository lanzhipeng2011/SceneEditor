
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

public class EditorHelper
{
	
	public static Vector3 GetBoundsCenter(GameObject target)
	{
		return GetBounds(target).center;
	}

	public static Bounds GetBounds(GameObject target)
	{
		Renderer[] mrs = target.GetComponentsInChildren<Renderer>();
		Bounds bounds = new Bounds();
		if (mrs.Length>0) {
			bounds = mrs[0].bounds;
		}
        for (int i = 0; i < mrs.Length; i++)
        {
			bounds.Encapsulate(mrs[i].bounds);
		}
		return bounds;
	}

	
	public Bounds GetLocalBounds(GameObject target, bool include_children = true)
	{
		MeshFilter[] mfs = target.gameObject.GetComponentsInChildren<MeshFilter>();
		Bounds bounds = new Bounds();
		if (mfs.Length > 0)
		{
			bounds = mfs[0].mesh.bounds;
		}
		for (int i = 0; i < mfs.Length; i++)
		{
			bounds.Encapsulate(mfs[i].mesh.bounds);
		}
		return bounds;
	}


	/// <summary>
	/// 射线拾取三角面
	/// </summary>
	/// <param name="ray"></param>
	/// <param name="meshCollider"></param>
	/// <param name="triangleIndex"></param>
	/// <returns></returns>
	public static bool TryPickTriangle(Ray ray, out MeshCollider meshCollider, out int triangleIndex)
	{
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000))
		{
			meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
			{
				triangleIndex = -1;
				return false;
			}

			//hit.triangleIndex和mesh.triangles 对应关系为：
			//triangles[triangleIndex*3]=第一个顶点
			//triangles[triangleIndex*3+1]=第一个顶点
			//triangles[triangleIndex*3+2]=第一个顶点

			triangleIndex = hit.triangleIndex; 

			return true;
		}
		else
		{
			meshCollider = null;
			triangleIndex = -1;
			return false;
		}
	}


	/// <summary>
	/// 射线拾取模型顶点
	/// </summary>
	/// <param name="ray"></param>
	/// <param name="meshCollider"></param>
	/// <param name="triangleIndex"></param>
	/// <returns></returns>
	public static bool TryPickTriangleVertice(Ray ray, out MeshCollider meshCollider, out Vector3 point)
	{
		int triangleIndex = -1;
		point = Vector3.zero;
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 10000))
		{
			meshCollider = hit.collider as MeshCollider;




			if (meshCollider == null || meshCollider.sharedMesh == null)
			{
				triangleIndex = -1;
				return false;
			}

			//hit.triangleIndex和mesh.triangles 对应关系为：
			//triangles[triangleIndex*3]=第一个顶点
			//triangles[triangleIndex*3+1]=第一个顶点
			//triangles[triangleIndex*3+2]=第一个顶点
			triangleIndex = hit.triangleIndex;  

			Mesh mesh = meshCollider.sharedMesh;
			Vector3[] vertices = mesh.vertices;
			int[] triangles = mesh.triangles;  //索引

			Vector3 p0 = meshCollider.transform.TransformPoint(vertices[triangles[triangleIndex * 3]]) ;
			Vector3 p1 = meshCollider.transform.TransformPoint(vertices[triangles[triangleIndex * 3 + 1]]);
			Vector3 p2 = meshCollider.transform.TransformPoint(vertices[triangles[triangleIndex * 3 + 2]]);

			


			float dis = (hit.point - p0).magnitude;
			point = p0;
			if ((hit.point - p1).magnitude < dis)
			{
				dis = (hit.point - p1).magnitude;
				point = p1;
			}


			if ((hit.point - p2).magnitude < dis)
			{

				point = p2;
			}

			return true;
		}
		else
		{
			meshCollider = null;
			triangleIndex = -1;
			return false;
		}
	}
	/// <summary>
	/// 误差一定范围内的认为是同一个顶点
	/// </summary>
	/// <param name="v1"></param>
	/// <param name="v2"></param>
	/// <returns></returns>
	public static bool IsSameVertex(Vector3 v1, Vector3 v2)
	{
		//return (v1 - v2).sqrMagnitude < 0.000001f;


		return Math.Abs((v1.x - v2.x)) < 0.00001f || Math.Abs((v1.y - v2.y)) < 0.00001f || Math.Abs((v1.z - v2.z)) < 0.00001f;
	}

	/// <summary>
	/// 根据一个三角面找到
	/// </summary>
	/// <param name="meshCollider"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	public static HashSet<int> GetPlaneTrianglesByFaceIndex(MeshCollider meshCollider, int index)
	{
		Vector3[] vertices = meshCollider.sharedMesh.vertices;
		int[] triangles = meshCollider.sharedMesh.triangles;  //索引

		HashSet<int> outTriangles = new HashSet<int>();
		outTriangles.Add(index);

		//三角面的顶点索引
		HashSet<int> vIndex = new HashSet<int>();
		vIndex.Add(triangles[index * 3]);
		vIndex.Add(triangles[index * 3 + 1]);
		vIndex.Add(triangles[index * 3 + 2]);

		//这个三角面的法线
		Vector3 p0 = vertices[triangles[index * 3]];
		Vector3 p1 = vertices[triangles[index * 3 + 1]];
		Vector3 p2 = vertices[triangles[index * 3 + 2]];
		Vector3 vDir = Vector3.Cross(p1 - p0, p2 - p1).normalized;

		//查找所有公用同一个三角面顶点的面
		HashSet<int> neighbour = new HashSet<int>();
		for (int i = 0; i < triangles.Length; i += 3)
		{
			if (i / 3 != index)
			{
				Vector3 t1 = vertices[triangles[i]];
				Vector3 t2 = vertices[triangles[i + 1]];
				Vector3 t3 = vertices[triangles[i + 2]];


				if (IsSameVertex(t1, p0) || IsSameVertex(t2, p0) || IsSameVertex(t3, p0) ||
					IsSameVertex(t1, p1) || IsSameVertex(t2, p1) || IsSameVertex(t3, p1) ||
					IsSameVertex(t1, p2) || IsSameVertex(t2, p2) || IsSameVertex(t3, p2))
				{
					if (!neighbour.Contains(i / 3))
						neighbour.Add(i / 3);


				}

				
			}
		}

		var threshold = Mathf.Cos(0.1f * Mathf.Deg2Rad);   //<0.1的都算
														  
		foreach (var nei in neighbour)
		{
			p0 = vertices[triangles[nei * 3]];
			p1 = vertices[triangles[nei * 3 + 1]];
			p2 = vertices[triangles[nei * 3 + 2]];

			Vector3 dir = Vector3.Cross(p1 - p0, p2 - p1).normalized;
			if (Vector3.Dot(vDir, dir) > threshold)
				outTriangles.Add(nei);
		}

		return outTriangles;
	}

	

	public static void GetVerticalRay(Vector3 p0, Vector3 p1, Vector3 p2, out Vector3 center, out Vector3 dir)
	{
		center = (p0 + p1 + p2) / 3;
		dir = Vector3.Cross(p1 - p0, p2 - p1).normalized;
	}

	/// <summary>
	/// 获取Mesh上多个三角面的中心点
	/// </summary>
	/// <param name="mesh"></param>
	/// <param name="trianglesIndex">三角面索引集合</param>
	/// <returns></returns>
	public static Vector3 GetMeshLocalCenter(Mesh mesh, HashSet<int> trianglesIndex)
	{
		Vector3 vResult = Vector3.zero;
		if (trianglesIndex.Count == 0)
			return vResult;

		foreach (var index in trianglesIndex)
		{
			vResult += mesh.vertices[mesh.triangles[index * 3]];
			vResult += mesh.vertices[mesh.triangles[index * 3 + 1]];
			vResult += mesh.vertices[mesh.triangles[index * 3 + 2]];
		}

		vResult /= trianglesIndex.Count * 3;
		return vResult;
	}

	/// <summary>
	/// 沿着连线将from按三角面贴附到to
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void AttachTwoGameObject(GameObject from, GameObject to)
	{
		Collider colliderFrom = from.GetComponent<Collider>();
		Collider colliderTo = to.GetComponent<Collider>();
		if (colliderFrom == null || colliderTo == null)
			return;

		Vector3 vFrom = from.transform.position;
		Vector3 vTo = to.transform.position;
		Vector3 vDir = (vTo - vFrom).normalized;
		RaycastHit[] hitsFrom = Physics.RaycastAll(new Ray(vFrom, vDir), 1000);

		RaycastHit rhTo = new RaycastHit();
		bool bFound = false;
		//找到接触点
		foreach (var hit in hitsFrom)
		{
			if (hit.transform == to.transform)
			{
				rhTo = hit;
				bFound = true;
				break;
			}
		}
		if (bFound == false)
			return;

		//to打出来的射线
		RaycastHit[] hitTo = Physics.RaycastAll(new Ray(vTo, -vDir), 1000);
		RaycastHit rhFrom = new RaycastHit();
		bFound = false;
		foreach (var hit in hitTo)
		{
			if (hit.transform == from.transform)
			{
				rhFrom = hit;
				bFound = true;
				break;
			}
		}
		if (bFound == false)
			return;

		float fDistance = (rhTo.point - rhFrom.point).magnitude;
		
		from.transform.position +=  vDir * fDistance;

	}


	/// <summary>
	/// 根据选择的模型顶点吸附物体
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void AttachTwoGameObjectByVertice(GameObject from, GameObject to,Vector3 fromVertice, Vector3 toVertice)
	{
		//将模型空间的顶点转换成世界空间顶点

		Vector3 fromWorldVertice = from.transform.TransformPoint(fromVertice);

		Vector3 toWorldVertice = from.transform.TransformPoint(toVertice);

		Vector3 fromTotoDir = toWorldVertice - fromWorldVertice;


		from.transform.position += fromTotoDir;

	}
}
