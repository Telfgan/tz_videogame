#if HE_SYSCORE
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics.API
{
    /// <summary>
    /// Interface for dealing with ballistic problems such as resolving a firing solution accounting for the parabolic trajectory of a projectile
    /// </summary>
    public static class Ballistics
    {
        /// <summary>
        /// Find the max range of a ballistic projectile
        /// </summary>
        /// <param name="speed">projectile speed</param>
        /// <param name="gravity">gravity magnitude</param>
        /// <param name="height">hight above</param>
        /// <returns></returns>
        public static float MaxRange(float speed, float gravity, float height)
        {
            if (speed <= 0 || gravity <= 0 || height < 0)
                return 0;

            var ang = 45 * Mathf.Deg2Rad;
            var cos = Mathf.Cos(ang);
            var sin = Mathf.Sin(ang);

            return (speed * cos / gravity) * (speed * sin + Mathf.Sqrt(speed * speed * sin * sin + 2 * gravity * height));
        }
        /// <summary>
        /// Finds the flight time given the hight difference between the target and source
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="height"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static float FlightTime(Vector3 start, Vector3 end, Vector3 velocity, Vector3 gravity)
        {
            Vector3 displacement = end - start;
            float xTime = displacement.x / velocity.x + displacement.z / velocity.z;
            float discriminant = velocity.y * velocity.y - 2f * gravity.y * displacement.y;
            if (discriminant < 0f)
            {
                return float.NaN;
            }

            float yTime1 = (velocity.y + Mathf.Sqrt(discriminant)) / gravity.y;
            float yTime2 = (velocity.y - Mathf.Sqrt(discriminant)) / gravity.y;

            float yTime;
            if (yTime1 > 0f)
            {
                yTime = yTime1;
            }
            else if (yTime2 > 0f)
            {
                yTime = yTime2;
            }
            else
            {
                return float.NaN;
            }

            return xTime + yTime;
        }
        /// <summary>
        /// Finds the flight time given the hight difference between the target and source
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="height"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static float FlightTime2D(Vector2 start, Vector2 end, Vector2 velocity, Vector2 gravity)
        {
            Vector2 displacement = end - start;
            float xTime = displacement.x / velocity.x;
            float discriminant = velocity.y * velocity.y - 2f * gravity.y * displacement.y;
            if (discriminant < 0f)
            {
                return float.NaN;
            }

            float yTime1 = (velocity.y + Mathf.Sqrt(discriminant)) / gravity.y;
            float yTime2 = (velocity.y - Mathf.Sqrt(discriminant)) / gravity.y;

            float yTime;
            if (yTime1 > 0f)
            {
                yTime = yTime1;
            }
            else if (yTime2 > 0f)
            {
                yTime = yTime2;
            }
            else
            {
                return float.NaN;
            }

            return xTime + yTime;
        }
        /// <summary>
        /// Finds the final velocity of a projectile
        /// </summary>
        /// <param name="initialVelocity">The initial velocity of the projectile</param>
        /// <param name="constantAcceleration">The gravity vector</param>
        /// <param name="flightTime">The total time in flight</param>
        /// <returns></returns>
        public static Vector3 FinalVelocity(Vector3 initialVelocity, Vector3 constantAcceleration, float flightTime)
        {
            var gravNorm = constantAcceleration.normalized;
            var planarVelocity = initialVelocity - new Vector3(initialVelocity.x * gravNorm.x, initialVelocity.y * gravNorm.y, initialVelocity.z * gravNorm.z);
            var initialVerticalVelocity = initialVelocity - planarVelocity;
            return planarVelocity + initialVerticalVelocity + (constantAcceleration * flightTime);
        }
        /// <summary>
        /// Finds the final velocity of a projectile
        /// </summary>
        /// <param name="initialVelocity">The initial velocity of the projectile</param>
        /// <param name="constantAcceleration">The gravity vector</param>
        /// <param name="flightTime">The total time in flight</param>
        /// <returns></returns>
        public static Vector2 FinalVelocity2D(Vector2 initialVelocity, Vector2 constantAcceleration, float flightTime)
        {
            var gravNorm = constantAcceleration.normalized;
            var planarVelocity = initialVelocity - new Vector2(initialVelocity.x * gravNorm.x, initialVelocity.y * gravNorm.y);
            var initialVerticalVelocity = initialVelocity - planarVelocity;
            return planarVelocity + initialVerticalVelocity + (constantAcceleration * flightTime);
        }
        /// <summary>
        /// Finds the low and high angle rotations to put a projectile on a target given an initial "speed" aka muzzle velocity (we don't call it velocity because its not a Vector)
        /// </summary>
        /// <param name="projectile">The starting position of the projectile where it will enter free flight, this is typically the muzzle of a gun/cannon or the point of release for a thrown object</param>
        /// <param name="speed">This is the linear speed of the projectile when it is released you can think of this as the magnitude of the muzzle velocity</param>
        /// <param name="target">The target to be aimed at</param>
        /// <param name="constantAcceleration">The sum of all constant accelerations, this would be at least gravity but could also include Coriolis and or Magnus effects</param>
        /// <param name="lowAngle">The resulting low angle rotation</param>
        /// <param name="highAngle">The resulting high angle rotation</param>
        /// <returns>The number of solutions found, can be 0 indicating no valid solution</returns>
        public static int Solution(Vector3 projectile, float speed, Vector3 target, Vector3 constantAcceleration, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            if (speed <= 0 || constantAcceleration == Vector3.zero)
                return 0;

            // Calculate rotation to align constant acceleration with standard downward direction
            Quaternion alignRotation = Quaternion.FromToRotation(constantAcceleration.normalized, Vector3.down);

            // Rotate projectile and target by alignRotation
            Vector3 alignedProjectile = alignRotation * projectile;
            Vector3 alignedTarget = alignRotation * target;

            // Calculate gravity magnitude
            float gravity = constantAcceleration.magnitude;

            // Calculate solutions using aligned vectors
            int numSolutions = Solution(alignedProjectile, speed, alignedTarget, gravity, out lowAngle, out highAngle);

            // Rotate resulting quaternions back to original orientation
            lowAngle = Quaternion.Inverse(alignRotation) * lowAngle;
            highAngle = Quaternion.Inverse(alignRotation) * highAngle;

            return numSolutions;
        }
        /// <summary>
        /// Finds the low and high angle rotations to put a projectile on a target given an initial "speed" aka muzzle velocity (we don't call it velocity because its not a Vector)
        /// </summary>
        /// <param name="projectile">The starting position of the projectile where it will enter free flight, this is typically the muzzle of a gun/cannon or the point of release for a thrown object</param>
        /// <param name="speed">This is the linear speed of the projectile when it is released you can think of this as the magnitude of the muzzle velocity</param>
        /// <param name="target">The target to be aimed at</param>
        /// <param name="constantAcceleration">The sum of all constant accelerations, this would be at least gravity but could also include Coriolis and or Magnus effects</param>
        /// <param name="lowAngle">The resulting low angle rotation</param>
        /// <param name="highAngle">The resulting high angle rotation</param>
        /// <returns>The number of solutions found, can be 0 indicating no valid solution</returns>
        public static int Solution2D(Vector2 projectile, float speed, Vector2 target, Vector2 constantAcceleration, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            if (speed <= 0 || constantAcceleration == Vector2.zero)
                return 0;

            // Calculate rotation to align constant acceleration with standard downward direction
            Quaternion alignRotation = Quaternion.FromToRotation(constantAcceleration.normalized, Vector3.down);

            // Rotate projectile and target by alignRotation
            Vector2 alignedProjectile = (Vector2)(alignRotation * (Vector3)projectile);
            Vector2 alignedTarget = (Vector2)(alignRotation * (Vector3)target);

            // Calculate gravity magnitude
            float gravity = constantAcceleration.magnitude;

            // Calculate solutions using aligned vectors
            int numSolutions = Solution2D(alignedProjectile, speed, alignedTarget, gravity, out lowAngle, out highAngle);

            // Rotate resulting quaternions back to original orientation
            lowAngle = Quaternion.Inverse(alignRotation) * lowAngle;
            highAngle = Quaternion.Inverse(alignRotation) * highAngle;

            return numSolutions;
        }
        /// <summary>
        /// Solves for two possible rotations to hit a fixed target from a fixed position.
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        /// <param name="gravity"></param>
        /// <param name="lowAngle"></param>
        /// <param name="highAngle"></param>
        /// <returns></returns>
        public static int Solution(Vector3 projectile, float speed, Vector3 target, float gravity, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            if (speed <= 0 || gravity <= 0)
                return 0;

            //Find the heading aka the distance and direction to the target
            var heading = target - projectile;
            var planar = new Vector3(heading.x, 0, heading.z);
            var linearDistance = planar.magnitude;

            var speed2 = speed * speed;
            var speed4 = speed * speed * speed * speed;
            var y = heading.y;
            var x = linearDistance;
            var gravX = gravity * x;

            //Calculate the discriminant of the quadratic equation
            float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

            //If discriminant is negative, no solution exists
            if (root < 0)
                return 0;

            //Find the root
            root = Mathf.Sqrt(root);
            var lowAng = Mathf.Atan2(speed2 - root, gravX);
            var highAng = Mathf.Atan2(speed2 + root, gravX);
            var sols = lowAng == highAng ? 1 : 2;

            var planarDir = planar.normalized;

            //Find the direction of the low angle
            var sDir = planarDir * Mathf.Cos(lowAng) * speed + Mathf.Sin(lowAng) * speed * Vector3.up;
            lowAngle = Quaternion.LookRotation(sDir);

            //If we have 2 angles find the high angle
            if (sols == 2)
            {
                sDir = planarDir * Mathf.Cos(highAng) * speed + Mathf.Sin(highAng) * speed * Vector3.up;
                highAngle = Quaternion.LookRotation(sDir);
            }
            else
                highAngle = lowAngle;

            return sols;
        }
        /// <summary>
        /// Solves for two possible rotations to hit a fixed target from a fixed position.
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        /// <param name="gravity"></param>
        /// <param name="lowAngle"></param>
        /// <param name="highAngle"></param>
        /// <returns></returns>
        public static int Solution2D(Vector2 projectile, float speed, Vector2 target, float gravity, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            if (speed <= 0 || gravity <= 0)
                return 0;

            var heading = target - projectile;
            var planar = new Vector2(heading.x, 0);
            var linearDistance = planar.magnitude;

            var speed2 = speed * speed;
            var speed4 = speed * speed * speed * speed;
            var y = heading.y;
            var x = linearDistance;
            var gravX = gravity * x;

            float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

            if (root < 0)
                return 0;

            root = Mathf.Sqrt(root);

            var lowAng = Mathf.Atan2(speed2 - root, gravX);
            var highAng = Mathf.Atan2(speed2 + root, gravX);
            var sols = lowAng == highAng ? 1 : 2;

            var planarDir = planar.normalized;

            var sDir = planarDir * Mathf.Cos(lowAng) * speed + Vector2.up * Mathf.Sin(lowAng) * speed;
            lowAngle = Quaternion.LookRotation(Vector3.forward, sDir);

            if (sols == 2)
            {
                sDir = planarDir * Mathf.Cos(highAng) * speed + Vector2.up * Mathf.Sin(highAng) * speed;
                highAngle = Quaternion.LookRotation(Vector3.forward, sDir);
            }
            else
                highAngle = lowAngle;

            return sols;
        }
        /// <summary>
        /// Solves for two possible rotations to hit a moving target from a fixed position
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        /// <param name="targetVelocity"></param>
        /// <param name="gravity"></param>
        /// <param name="lowAngle"></param>
        /// <param name="highAngle"></param>
        /// <returns></returns>
        public static int Solution(Vector3 projectile, float speed, Vector3 target, Vector3 targetVelocity, float gravity, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            double doubleGravity = gravity;

            double doubleProjectileX = projectile.x;
            double doubleProjectileY = projectile.y;
            double doubleProjectileZ = projectile.z;
            double doubleTargetX = target.x;
            double doubleTargetY = target.y;
            double doubleTargetZ = target.z;
            double doubleTargetVelocityX = targetVelocity.x;
            double doubleTargetVelocityY = targetVelocity.y;
            double doubleTargetVelocityZ = targetVelocity.z;
            double doubleSpeed = speed;

            double doubleHeadingX = doubleTargetX - doubleProjectileX;
            double doubleHeadingZ = doubleTargetZ - doubleProjectileZ;
            double doubleHeadingY = doubleTargetY - doubleProjectileY;
            double doubleHalfGravity = -.5f * doubleGravity;

            // Quartic Coeffecients
            double c0 = doubleHalfGravity * doubleHalfGravity;
            double c1 = -2 * doubleTargetVelocityY * doubleHalfGravity;
            double c2 = doubleTargetVelocityY * doubleTargetVelocityY - 2 * doubleHeadingY * doubleHalfGravity - doubleSpeed * doubleSpeed + doubleTargetVelocityX * doubleTargetVelocityX + doubleTargetVelocityZ * doubleTargetVelocityZ;
            double c3 = 2 * doubleHeadingY * doubleTargetVelocityY + 2 * doubleHeadingX * doubleTargetVelocityX + 2 * doubleHeadingZ * doubleTargetVelocityZ;
            double c4 = doubleHeadingY * doubleHeadingY + doubleHeadingX * doubleHeadingX + doubleHeadingZ * doubleHeadingZ;

            // Solve quartic
            double[] times = new double[4];
            int numTimes = Maths.SolveQuartic(c0, c1, c2, c3, c4, out times[0], out times[1], out times[2], out times[3]);

            // Sort so faster collision is found first
            System.Array.Sort(times);

            // Plug quartic solutions into base equations
            Vector3[] solutions = new Vector3[2];
            int numSolutions = 0;

            for (int i = 0; i < times.Length && numSolutions < 2; ++i)
            {
                double t = times[i];
                if (t <= 0 || double.IsNaN(t))
                    continue;

                solutions[numSolutions].x = (float)((doubleHeadingX + doubleTargetVelocityX * t) / t);
                solutions[numSolutions].y = (float)((doubleHeadingY + doubleTargetVelocityY * t - doubleHalfGravity * t * t) / t);
                solutions[numSolutions].z = (float)((doubleHeadingZ + doubleTargetVelocityZ * t) / t);
                ++numSolutions;
            }

            if (numSolutions > 1)
            {
                lowAngle = Quaternion.LookRotation(solutions[0]);
                highAngle = Quaternion.LookRotation(solutions[1]);
            }
            else if (numSolutions > 0)
            {
                lowAngle = Quaternion.LookRotation(solutions[0]);
                highAngle = lowAngle;
            }

            return numSolutions;
        }
        /// <summary>
        /// Solves for two possible rotations to hit a moving target from a fixed position
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        /// <param name="targetVelocity"></param>
        /// <param name="gravity"></param>
        /// <param name="lowAngle"></param>
        /// <param name="highAngle"></param>
        /// <returns></returns>
        public static int Solution2D(Vector2 projectile, float speed, Vector2 target, Vector2 targetVelocity, float gravity, out Quaternion lowAngle, out Quaternion highAngle)
        {
            lowAngle = Quaternion.identity;
            highAngle = Quaternion.identity;

            double doubleGravity = gravity;

            double doubleProjectileX = projectile.x;
            double doubleProjectileY = projectile.y;
            double doubleTargetX = target.x;
            double doubleTargetY = target.y;
            double doubleTargetVelocityX = targetVelocity.x;
            double doubleTargetVelocityY = targetVelocity.y;
            double doubleSpeed = speed;

            double doubleHeadingX = doubleTargetX - doubleProjectileX;
            double doubleHeadingY = doubleTargetY - doubleProjectileY;
            double doubleHalfGravity = -.5f * doubleGravity;

            // Quartic Coeffecients
            double c0 = doubleHalfGravity * doubleHalfGravity;
            double c1 = -2 * doubleTargetVelocityY * doubleHalfGravity;
            double c2 = doubleTargetVelocityY * doubleTargetVelocityY - 2 * doubleHeadingY * doubleHalfGravity - doubleSpeed * doubleSpeed + doubleTargetVelocityX * doubleTargetVelocityX;
            double c3 = 2 * doubleHeadingY * doubleTargetVelocityY + 2 * doubleHeadingX * doubleTargetVelocityX;
            double c4 = doubleHeadingY * doubleHeadingY + doubleHeadingX * doubleHeadingX;

            // Solve quartic
            double[] times = new double[4];
            int numTimes = Maths.SolveQuartic(c0, c1, c2, c3, c4, out times[0], out times[1], out times[2], out times[3]);

            // Sort so faster collision is found first
            System.Array.Sort(times);

            // Plug quartic solutions into base equations
            Vector2[] solutions = new Vector2[2];
            int numSolutions = 0;

            for (int i = 0; i < times.Length && numSolutions < 2; ++i)
            {
                double t = times[i];
                if (t <= 0 || double.IsNaN(t))
                    continue;

                solutions[numSolutions].x = (float)((doubleHeadingX + doubleTargetVelocityX * t) / t);
                solutions[numSolutions].y = (float)((doubleHeadingY + doubleTargetVelocityY * t - doubleHalfGravity * t * t) / t);
                ++numSolutions;
            }

            if (numSolutions > 1)
            {
                lowAngle = Quaternion.LookRotation(solutions[0]);
                highAngle = Quaternion.LookRotation(solutions[1]);
            }
            else if (numSolutions > 0)
            {
                lowAngle = Quaternion.LookRotation(solutions[0]);
                highAngle = lowAngle;
            }

            return numSolutions;
        }
        /// <summary>
        /// Solves for the required velocity and gravity to apply to a projectile to hit the target at a perscribed max height.
        /// This is used to plot a trajectory with a defined max height
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="linearSpeed">speed across the XZ plane</param>
        /// <param name="target"></param>
        /// <param name="arcCeiling"></param>
        /// <param name="firingVelocity"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static bool Solution(Vector3 projectile, float linearSpeed, Vector3 target, float arcCeiling, out Vector3 firingVelocity, out float gravity)
        {
            firingVelocity = Vector3.zero;
            gravity = 0;

            if (projectile == target || linearSpeed <= 0 || arcCeiling <= projectile.y)
                return false;

            Vector3 diff = target - projectile;
            Vector3 planar = new Vector3(diff.x, 0f, diff.z);
            float linearDistance = planar.magnitude;

            if (linearDistance == 0)
                return false;

            float time = linearDistance / linearSpeed;

            firingVelocity = planar.normalized * linearSpeed;

            // System of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = projectile.y;       // initial
            float b = arcCeiling;       // peak
            float c = target.y;     // final

            gravity = -4 * (a - 2 * b + c) / (time * time);
            firingVelocity.y = -(3 * a - 4 * b + c) / time;

            return true;
        }
        /// <summary>
        /// Solves for the required velocity and gravity to apply to a projectile to hit the target at a perscribed max height.
        /// This is used to plot a trajectory with a defined max height
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        /// <param name="projectile"></param>
        /// <param name="linearSpeed">speed across the XZ plane</param>
        /// <param name="target"></param>
        /// <param name="arcCeiling"></param>
        /// <param name="firingVelocity"></param>
        /// <param name="gravity"></param>
        /// <returns></returns>
        public static bool Solution2D(Vector2 projectile, float linearSpeed, Vector2 target, float arcCeiling, out Vector2 firingVelocity, out float gravity)
        {
            firingVelocity = Vector2.zero;
            gravity = 0;

            if (projectile == target || linearSpeed <= 0 || arcCeiling <= projectile.y)
                return false;

            Vector2 diff = target - projectile;
            Vector2 planar = new Vector3(diff.x, 0f);
            float linearDistance = planar.magnitude;

            if (linearDistance == 0)
                return false;

            float time = linearDistance / linearSpeed;

            firingVelocity = planar.normalized * linearSpeed;

            // System of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = projectile.y;       // initial
            float b = arcCeiling;       // peak
            float c = target.y;     // final

            gravity = -4 * (a - 2 * b + c) / (time * time);
            firingVelocity.y = -(3 * a - 4 * b + c) / time;

            return true;
        }
        /// <summary>
        /// Solves for the required velocity and gravity to apply to a projectile to hit the target at a perscribed max height.
        /// This is used to plot a trajectory with a defined max height
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        public static bool Solution(Vector3 projectile, float linearSpeed, Vector3 target, Vector3 targetVelocity, float arcCeiling, out Vector3 firingVelocity, out float gravity, out Vector3 impactPoint)
        {
            firingVelocity = Vector3.zero;
            gravity = 0f;
            impactPoint = Vector3.zero;

            if (projectile == target || linearSpeed <= 0)
                return false;

            // Ground plane terms
            Vector3 plane = new Vector3(targetVelocity.x, 0f, targetVelocity.z);
            Vector3 planeDiff = target - projectile;
            planeDiff.y = 0;

            // Derivation
            //   (1) Base formula: |P + V*t| = S*t
            //   (2) Substitute variables: |diffXZ + targetVelXZ*t| = S*t
            //   (3) Square both sides: Dot(diffXZ,diffXZ) + 2*Dot(diffXZ, targetVelXZ)*t + Dot(targetVelXZ, targetVelXZ)*t^2 = S^2 * t^2
            //   (4) Quadratic: (Dot(targetVelXZ,targetVelXZ) - S^2)t^2 + (2*Dot(diffXZ, targetVelXZ))*t + Dot(diffXZ, diffXZ) = 0
            float c0 = Vector3.Dot(plane, plane) - linearSpeed * linearSpeed;
            float c1 = 2f * Vector3.Dot(planeDiff, plane);
            float c2 = Vector3.Dot(planeDiff, planeDiff);
            double t0, t1;
            int n = Maths.SolveQuadric(c0, c1, c2, out t0, out t1);

            // pick smallest, positive time
            bool valid0 = n > 0 && t0 > 0;
            bool valid1 = n > 1 && t1 > 0;

            float t;
            if (!valid0 && !valid1)
                return false;
            else if (valid0 && valid1)
                t = Mathf.Min((float)t0, (float)t1);
            else
                t = valid0 ? (float)t0 : (float)t1;

            // Calculate impact point
            impactPoint = target + (targetVelocity * t);

            // Calculate fire velocity along XZ plane
            Vector3 dir = impactPoint - projectile;
            firingVelocity = new Vector3(dir.x, 0f, dir.z).normalized * linearSpeed;

            // Solve system of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = projectile.y;       // initial
            float b = Mathf.Max(projectile.y, impactPoint.y) + arcCeiling;  // peak
            float c = impactPoint.y;   // final

            gravity = -4 * (a - 2 * b + c) / (t * t);
            firingVelocity.y = -(3 * a - 4 * b + c) / t;

            return true;
        }
        /// <summary>
        /// Solves for the required velocity and gravity to apply to a projectile to hit the target at a perscribed max height.
        /// This is used to plot a trajectory with a defined max height
        /// </summary>
        /// <remarks>
        /// Derived from:
        /// https://github.com/forrestthewoods/lib_fts/blob/master/code/fts_ballistic_trajectory.cs
        /// </remarks>
        public static bool Solution2D(Vector2 projectile, float linearSpeed, Vector2 target, Vector2 targetVelocity, float arcCeiling, out Vector2 firingVelocity, out float gravity, out Vector2 impactPoint)
        {
            firingVelocity = Vector2.zero;
            gravity = 0f;
            impactPoint = Vector2.zero;

            if (projectile == target || linearSpeed <= 0)
                return false;

            // Ground plane terms
            Vector2 plane = new Vector2(targetVelocity.x, 0f);
            Vector2 planeDiff = target - projectile;
            planeDiff.y = 0;

            // Derivation
            //   (1) Base formula: |P + V*t| = S*t
            //   (2) Substitute variables: |diffXZ + targetVelXZ*t| = S*t
            //   (3) Square both sides: Dot(diffXZ,diffXZ) + 2*Dot(diffXZ, targetVelXZ)*t + Dot(targetVelXZ, targetVelXZ)*t^2 = S^2 * t^2
            //   (4) Quadratic: (Dot(targetVelXZ,targetVelXZ) - S^2)t^2 + (2*Dot(diffXZ, targetVelXZ))*t + Dot(diffXZ, diffXZ) = 0
            float c0 = Vector3.Dot(plane, plane) - linearSpeed * linearSpeed;
            float c1 = 2f * Vector3.Dot(planeDiff, plane);
            float c2 = Vector3.Dot(planeDiff, planeDiff);
            double t0, t1;
            int n = Maths.SolveQuadric(c0, c1, c2, out t0, out t1);

            // pick smallest, positive time
            bool valid0 = n > 0 && t0 > 0;
            bool valid1 = n > 1 && t1 > 0;

            float t;
            if (!valid0 && !valid1)
                return false;
            else if (valid0 && valid1)
                t = Mathf.Min((float)t0, (float)t1);
            else
                t = valid0 ? (float)t0 : (float)t1;

            // Calculate impact point
            impactPoint = target + (targetVelocity * t);

            // Calculate fire velocity along XZ plane
            Vector2 dir = impactPoint - projectile;
            firingVelocity = new Vector3(dir.x, 0f).normalized * linearSpeed;

            // Solve system of equations. Hit max_height at t=.5*time. Hit target at t=time.
            //
            // peak = y0 + vertical_speed*halfTime + .5*gravity*halfTime^2
            // end = y0 + vertical_speed*time + .5*gravity*time^s
            // Wolfram Alpha: solve b = a + .5*v*t + .5*g*(.5*t)^2, c = a + vt + .5*g*t^2 for g, v
            float a = projectile.y;       // initial
            float b = Mathf.Max(projectile.y, impactPoint.y) + arcCeiling;  // peak
            float c = impactPoint.y;   // final

            gravity = -4 * (a - 2 * b + c) / (t * t);
            firingVelocity.y = -(3 * a - 4 * b + c) / t;

            return true;
        }
        /// <summary>
        /// Returns the velocity required to hit the target at the specified time
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target"></param>
        /// <param name="flightTime"></param>
        /// <returns></returns>
        public static Vector3 Solution(Vector3 projectile, Vector3 target, float gravity, float flightTime)
        {
            var heading = target - projectile;
            var planar = new Vector3(heading.x, 0, heading.z);

            float height = heading.y;
            float linearDistance = planar.magnitude;

            float rise = height / flightTime + 0.5f * gravity * flightTime;
            float linear = linearDistance / flightTime;

            var result = planar.normalized;
            result *= linear;
            result.y = rise;

            return result;
        }
        /// <summary>
        /// Returns the velocity required to hit the target at the specified time
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target"></param>
        /// <param name="flightTime"></param>
        /// <returns></returns>
        public static Vector2 Solution2D(Vector2 projectile, Vector2 target, float gravity, float flightTime)
        {
            var heading = target - projectile;
            var planar = new Vector2(heading.x, 0);

            float height = heading.y;
            float linearDistance = planar.magnitude;

            float rise = height / flightTime + 0.5f * gravity * flightTime;
            float linear = linearDistance / flightTime;

            var result = planar.normalized;
            result *= linear;
            result.y = rise;

            return result;
        }
        /// <summary>
        /// Finds the required speed of the projectile to hit the target given a firing direction
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target"></param>
        /// <param name="angle">the angle of launch</param>
        /// <param name="gravity"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static bool Solution(Vector3 projectile, Vector3 target, float angle, float gravity, out float speed)
        {
            var heading = target - projectile;
            var planar = new Vector3(heading.x, 0, heading.z);
            var range = planar.magnitude;

            var tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);
            var z = Mathf.Sqrt(-gravity * range * range / (2f * (heading.y - range * tanAlpha)));
            var y = tanAlpha * z;

            speed = new Vector3(0, y, z).magnitude;

            if (!float.IsNaN(speed))
                return true;
            else
                return false;
        }
        /// <summary>
        /// Finds the required speed of the projectile to hit the target given a firing direction
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="target"></param>
        /// <param name="angle">the angle of launch</param>
        /// <param name="gravity"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static bool Solution2D(Vector2 projectile, Vector2 target, float angle, float gravity, out float speed)
        {
            var heading = target - projectile;
            var planar = new Vector2(heading.x, 0);
            var range = planar.magnitude;

            var tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);
            var z = Mathf.Sqrt(-gravity * range * range / (2f * (heading.y - range * tanAlpha)));
            var y = tanAlpha * z;

            speed = new Vector3(0, y, z).magnitude;

            if (!float.IsNaN(speed))
                return true;
            else
                return false;
        }

        /// <summary>
        /// marches a ray cast along the trajectory checking for impact and tracing the path
        /// </summary>
        /// <param name="start">The world point to start from</param>
        /// <param name="velocity">The velocity (direction * speed) to march</param>
        /// <param name="constantAcceleration">The effect of gravity</param>
        /// <param name="resolution">The distance for each march to test</param>
        /// <param name="maxLength">The maximum length to test across</param>
        /// <param name="collisionLayers">The layers to test for collision with</param>
        /// <param name="hit">The hit information, if non then hit.transform will be null</param>
        /// <param name="path">A list of points along the path that where traversed includes the start and hit point</param>
        /// <param name="distance">The arc distance travailed</param>
        /// <returns>True if a collision occurred, false otherwise</returns>
        public static bool Raycast(Vector3 start, Vector3 velocity, Vector3 constantAcceleration, float resolution, float maxLength, LayerMask collisionLayers, out RaycastHit hit, out List<(Vector3 position, Vector3 velocity, float time)> path, out float distance)
        {
            path = new List<(Vector3 position, Vector3 velocity, float time)>
            {
                (start, velocity, 0f),
            };

            var currentPos = start;
            var currentVel = velocity;
            var timeSum = 0f;
            distance = 0f;

            Ray ray = new Ray(currentPos, currentVel.normalized);

            //Walk the ray to impact
            while (!Physics.Raycast(ray, out hit, resolution, collisionLayers)
                && Vector3.Distance(start, currentPos) < maxLength)
            {
                var t = resolution / currentVel.magnitude;
                timeSum += t;
                var s = currentPos;
                currentVel += t * constantAcceleration;
                currentPos += t * currentVel;
                distance += Vector2.Distance(s, currentPos);
                path.Add((currentPos, currentVel, timeSum));

                ray = new Ray(currentPos, currentVel.normalized);
            }

            if (hit.transform != null)
            {
                var target = ray.GetPoint(hit.distance);
                path.Add((target, currentVel, timeSum + ((target - currentPos).magnitude / currentVel.magnitude)));
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// marches a ray cast along the trajectory checking for impact and tracing the path
        /// </summary>
        /// <param name="start">The world point to start from</param>
        /// <param name="velocity">The velocity (direction * speed) to march</param>
        /// <param name="constantAcceleration">The effect of gravity</param>
        /// <param name="resolution">The distance for each march to test</param>
        /// <param name="maxLength">The maximum length to test across</param>
        /// <param name="collisionLayers">The layers to test for collision with</param>
        /// <param name="hit">The hit information, if non then hit.transform will be null</param>
        /// <param name="path">A list of points along the path that where traversed includes the start and hit point</param>
        /// <param name="distance">The arc distance travailed</param>
        /// <returns>True if a collision occurred, false otherwise</returns>
        public static bool SphereCast(Vector3 start, Collider startCollider, Vector3 velocity, Vector3 constantAcceleration, float radius, float resolution, float maxLength, LayerMask collisionLayers, out RaycastHit hit, out List<(Vector3 position, Vector3 velocity, float time)> path, out float distance)
        {
            path = new List<(Vector3 position, Vector3 velocity, float time)>
            {
                (start, velocity, 0f),
            };

            var currentPos = start;
            var currentVel = velocity;
            var timeSum = 0f;
            distance = 0f;

            if (startCollider != null)
                startCollider.enabled = false;

            Ray ray = new Ray(currentPos, currentVel.normalized);

            //Walk the ray to impact
            while (!Physics.SphereCast(ray, radius, out hit, resolution, collisionLayers)
                && Vector3.Distance(start, currentPos) < maxLength)
            {
                var t = resolution / currentVel.magnitude;
                timeSum += t;
                var s = currentPos;
                currentVel += t * constantAcceleration;
                currentPos += t * currentVel;
                distance += Vector2.Distance(s, currentPos);
                path.Add((currentPos, currentVel, timeSum));

                ray = new Ray(currentPos, currentVel.normalized);

                if(startCollider != null
                    && startCollider.enabled == false
                    && distance > radius)
                    startCollider.enabled = true;
            }

            if (startCollider != null)
                startCollider.enabled = true;

            if (hit.transform != null)
            {
                var target = ray.GetPoint(hit.distance);
                path.Add((target, currentVel, timeSum + ((target - currentPos).magnitude / currentVel.magnitude)));
                return true;
            }
            else
                return false;
        }
        public static bool SphereCast(Vector3 start, Collider startCollider, Vector3 velocity, float radius, float resolution, float maxLength, LayerMask collisionLayers, out RaycastHit hit, out List<(Vector3 position, Vector3 velocity, float time)> path, out float distance)
        {
            path = new List<(Vector3 position, Vector3 velocity, float time)>
            {
                (start, velocity, 0f),
            };

            var currentPos = start;
            var currentVel = velocity;
            var timeSum = 0f;
            distance = 0f;

            if (startCollider != null)
                startCollider.enabled = false;

            Ray ray = new Ray(currentPos, currentVel.normalized);

            var hitSomething = Physics.SphereCast(ray, radius, out hit, maxLength, collisionLayers);


            if (startCollider != null)
                startCollider.enabled = true;

            if (hitSomething && hit.transform != null)
            {
                var target = ray.GetPoint(hit.distance);
                path.Add((target, currentVel, timeSum + ((target - currentPos).magnitude / currentVel.magnitude)));
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// marches a ray cast along the trajectory checking for impact and tracing the path
        /// </summary>
        /// <param name="start">The world point to start from</param>
        /// <param name="velocity">The velocity (direction * speed) to march</param>
        /// <param name="constantAcceleration">The effect of gravity</param>
        /// <param name="resolution">The distance for each march to test</param>
        /// <param name="maxLength">The maximum length to test across</param>
        /// <param name="collisionLayers">The layers to test for collision with</param>
        /// <param name="hit">The hit information, if non then hit.transform will be null</param>
        /// <param name="path">A list of points along the path that where traversed includes the start and hit point</param>
        /// <param name="distance">The arc distance travailed</param>
        /// <returns>True if a collision occurred, false otherwise</returns>
        public static bool Raycast2D(Vector2 start, Vector2 velocity, Vector2 constantAcceleration, float resolution, float maxLength, LayerMask collisionLayers, out RaycastHit2D hit, out List<(Vector2 position, Vector2 velocity, float time)> path, out float distance)
        {
            path = new List<(Vector2 position, Vector2 velocity, float time)>
            {
                (start, velocity, 0f),
            };

            var currentPos = start;
            var currentVel = velocity;
            var timeSum = 0f;
            distance = 0f;

            hit = Physics2D.Raycast(currentPos, currentVel.normalized, resolution, collisionLayers);

            //Walk the ray to impact
            while (hit.collider == null
                && Vector3.Distance(start, currentPos) < maxLength)
            {
                var t = resolution / currentVel.magnitude;
                timeSum += t;
                var s = currentPos;
                currentVel += t * constantAcceleration;
                currentPos += t * currentVel;
                distance += Vector2.Distance(s, currentPos);
                path.Add((currentPos, currentVel, timeSum));

                hit = Physics2D.Raycast(currentPos, currentVel.normalized, resolution, collisionLayers);
            }

            if (hit.collider != null)
            {
                path.Add((hit.centroid, currentVel, timeSum + ((hit.centroid - currentPos).magnitude / currentVel.magnitude)));
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// marches a ray cast along the trajectory checking for impact and tracing the path
        /// </summary>
        /// <param name="start">The world point to start from</param>
        /// <param name="velocity">The velocity (direction * speed) to march</param>
        /// <param name="constantAcceleration">The effect of gravity</param>
        /// <param name="resolution">The distance for each march to test</param>
        /// <param name="maxLength">The maximum length to test across</param>
        /// <param name="collisionLayers">The layers to test for collision with</param>
        /// <param name="hit">The hit information, if non then hit.transform will be null</param>
        /// <param name="path">A list of points along the path that where traversed includes the start and hit point</param>
        /// <param name="distance">The arc distance travailed</param>
        /// <returns>True if a collision occurred, false otherwise</returns>
        public static bool CircleCast(Vector2 start, Collider2D startCollider, Vector2 velocity, Vector2 constantAcceleration, float radius, float resolution, float maxLength, LayerMask collisionLayers, out RaycastHit2D hit, out List<(Vector2 position, Vector2 velocity, float time)> path, out float distance)
        {
            path = new List<(Vector2 position, Vector2 velocity, float time)>
            {
                (start, velocity, 0f),
            };

            var currentPos = start;
            var currentVel = velocity;
            var timeSum = 0f;
            distance = 0f;

            if (startCollider != null)
                startCollider.enabled = false;

            hit = Physics2D.CircleCast(currentPos, radius, currentVel.normalized, resolution, collisionLayers);

            while (hit.collider == null
                && distance < maxLength)
            {
                var t = resolution / currentVel.magnitude;
                timeSum += t;
                var s = currentPos;
                currentVel += t * constantAcceleration;
                currentPos += t * currentVel;
                distance += Vector2.Distance(s, currentPos);
                path.Add((currentPos, currentVel, timeSum));
                
                hit = Physics2D.CircleCast(currentPos, radius, currentVel.normalized, resolution, collisionLayers);

                if (startCollider != null
                    && startCollider.enabled == false
                    && distance > radius)
                    startCollider.enabled = true;
            }

            if (startCollider != null)
                startCollider.enabled = true;

            if (hit.collider != null)
            {
                var t = (hit.centroid - currentPos).magnitude / currentVel.magnitude;
                path.Add((hit.centroid, currentVel, timeSum + t));
                return true;
            }
            else
                return false;
        }
    }
}
#endif