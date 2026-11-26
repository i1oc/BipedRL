using System;
using BipedRL.Editor.Urdf;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Symmetric 3x3 matrix, used for inertia tensors. Only the six independent values are stored.
    /// </summary>
    public readonly struct SymmetricMatrix3
    {
        public static readonly SymmetricMatrix3 Zero = new SymmetricMatrix3(0f, 0f, 0f, 0f, 0f, 0f);

        public float XX { get; }
        public float YY { get; }
        public float ZZ { get; }
        public float XY { get; }
        public float XZ { get; }
        public float YZ { get; }

        public SymmetricMatrix3(float xx, float yy, float zz, float xy, float xz, float yz)
        {
            XX = xx;
            YY = yy;
            ZZ = zz;
            XY = xy;
            XZ = xz;
            YZ = yz;
        }

        public static SymmetricMatrix3 FromInertia(UrdfInertia inertia)
        {
            return new SymmetricMatrix3(inertia.Ixx, inertia.Iyy, inertia.Izz, inertia.Ixy, inertia.Ixz, inertia.Iyz);
        }

        /// <summary>
        /// Inertia of a point mass at <paramref name="offset"/> about the origin:
        /// m (|d|² I - d dᵀ). This is the parallel axis theorem term.
        /// </summary>
        public static SymmetricMatrix3 PointMass(float mass, Vector3 offset)
        {
            float squaredLength = offset.sqrMagnitude;
            return new SymmetricMatrix3(
                mass * (squaredLength - offset.x * offset.x),
                mass * (squaredLength - offset.y * offset.y),
                mass * (squaredLength - offset.z * offset.z),
                -mass * offset.x * offset.y,
                -mass * offset.x * offset.z,
                -mass * offset.y * offset.z);
        }

        public static SymmetricMatrix3 operator +(SymmetricMatrix3 a, SymmetricMatrix3 b)
        {
            return new SymmetricMatrix3(a.XX + b.XX, a.YY + b.YY, a.ZZ + b.ZZ, a.XY + b.XY, a.XZ + b.XZ, a.YZ + b.YZ);
        }

        /// <summary>
        /// Expresses the tensor in the parent frame of <paramref name="rotation"/>: R M Rᵀ.
        /// </summary>
        public SymmetricMatrix3 Rotated(Quaternion rotation)
        {
            float[,] r = ToRotationMatrix(rotation);
            float[,] m = ToArray();
            var result = new float[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float sum = 0f;
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            sum += r[i, k] * m[k, l] * r[j, l];
                        }
                    }

                    result[i, j] = sum;
                }
            }

            return new SymmetricMatrix3(result[0, 0], result[1, 1], result[2, 2], result[0, 1], result[0, 2], result[1, 2]);
        }

        /// <summary>
        /// Finds the principal axes with the cyclic Jacobi method: <paramref name="moments"/>
        /// are the eigenvalues and <paramref name="rotation"/> turns the principal axes into
        /// this matrix's frame. This is the format ArticulationBody.inertiaTensor and
        /// inertiaTensorRotation expect.
        /// </summary>
        public void ToPrincipalAxes(out Vector3 moments, out Quaternion rotation)
        {
            double[,] a =
            {
                { XX, XY, XZ },
                { XY, YY, YZ },
                { XZ, YZ, ZZ }
            };
            double[,] v = { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            for (int sweep = 0; sweep < 50; sweep++)
            {
                double offDiagonal = a[0, 1] * a[0, 1] + a[0, 2] * a[0, 2] + a[1, 2] * a[1, 2];
                if (offDiagonal < 1e-20)
                {
                    break;
                }

                RotateToZero(a, v, 0, 1);
                RotateToZero(a, v, 0, 2);
                RotateToZero(a, v, 1, 2);
            }

            moments = new Vector3((float)a[0, 0], (float)a[1, 1], (float)a[2, 2]);

            var axisX = new Vector3((float)v[0, 0], (float)v[1, 0], (float)v[2, 0]);
            var axisY = new Vector3((float)v[0, 1], (float)v[1, 1], (float)v[2, 1]);
            var axisZ = new Vector3((float)v[0, 2], (float)v[1, 2], (float)v[2, 2]);

            // Eigenvectors can come out as a reflection; flipping one axis makes it a rotation.
            if (Vector3.Dot(axisX, Vector3.Cross(axisY, axisZ)) < 0f)
            {
                axisZ = -axisZ;
            }

            rotation = FromAxes(axisX, axisY, axisZ);
        }

        // One Jacobi rotation in the (p, q) plane that makes a[p, q] zero (Numerical Recipes, section 11.1).
        private static void RotateToZero(double[,] a, double[,] v, int p, int q)
        {
            if (Math.Abs(a[p, q]) < 1e-15)
            {
                return;
            }

            double theta = (a[q, q] - a[p, p]) / (2.0 * a[p, q]);
            double t = Math.Sign(theta == 0 ? 1 : theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1.0));
            double c = 1.0 / Math.Sqrt(t * t + 1.0);
            double s = t * c;

            for (int k = 0; k < 3; k++)
            {
                double akp = a[k, p];
                double akq = a[k, q];
                a[k, p] = c * akp - s * akq;
                a[k, q] = s * akp + c * akq;
            }

            for (int k = 0; k < 3; k++)
            {
                double apk = a[p, k];
                double aqk = a[q, k];
                a[p, k] = c * apk - s * aqk;
                a[q, k] = s * apk + c * aqk;
            }

            for (int k = 0; k < 3; k++)
            {
                double vkp = v[k, p];
                double vkq = v[k, q];
                v[k, p] = c * vkp - s * vkq;
                v[k, q] = s * vkp + c * vkq;
            }
        }

        private float[,] ToArray()
        {
            return new[,]
            {
                { XX, XY, XZ },
                { XY, YY, YZ },
                { XZ, YZ, ZZ }
            };
        }

        // Columns are the rotated X, Y and Z axes.
        private static float[,] ToRotationMatrix(Quaternion rotation)
        {
            Vector3 x = rotation * Vector3.right;
            Vector3 y = rotation * Vector3.up;
            Vector3 z = rotation * Vector3.forward;
            return new[,]
            {
                { x.x, y.x, z.x },
                { x.y, y.y, z.y },
                { x.z, y.z, z.z }
            };
        }

        // Standard conversion from a rotation matrix (columns = axes) to a quaternion.
        private static Quaternion FromAxes(Vector3 x, Vector3 y, Vector3 z)
        {
            float m00 = x.x, m01 = y.x, m02 = z.x;
            float m10 = x.y, m11 = y.y, m12 = z.y;
            float m20 = x.z, m21 = y.z, m22 = z.z;
            float trace = m00 + m11 + m22;

            if (trace > 0f)
            {
                float s = Mathf.Sqrt(trace + 1f) * 2f;
                return new Quaternion((m21 - m12) / s, (m02 - m20) / s, (m10 - m01) / s, 0.25f * s);
            }

            if (m00 > m11 && m00 > m22)
            {
                float s = Mathf.Sqrt(1f + m00 - m11 - m22) * 2f;
                return new Quaternion(0.25f * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s);
            }

            if (m11 > m22)
            {
                float s = Mathf.Sqrt(1f + m11 - m00 - m22) * 2f;
                return new Quaternion((m01 + m10) / s, 0.25f * s, (m12 + m21) / s, (m02 - m20) / s);
            }

            float s2 = Mathf.Sqrt(1f + m22 - m00 - m11) * 2f;
            return new Quaternion((m02 + m20) / s2, (m12 + m21) / s2, 0.25f * s2, (m10 - m01) / s2);
        }
    }
}
