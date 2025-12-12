#if HE_SYSCORE

using Unity.Mathematics;
using UnityEngine;

namespace HeathenEngineering.UnityPhysics.API
{
    /// <summary>
    /// A helper math library with commonly used physics functions.
    /// </summary>
    public static class Maths
    {
        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropriate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        /// <param name="targetSubject">the subject to target</param>
        /// <returns></returns>
        public static float3 FastIntercept(float3 position, float speed, Rigidbody targetSubject)
        {
            var distance = math.distance(position, targetSubject.position);
            return ValidateNaN(targetSubject.position + targetSubject.linearVelocity * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropriate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        public static float3 FastIntercept(float3 position, float speed, float3 targetPosition, float3 targetVelocity)
        {
            var distance = math.distance(position, targetPosition);
            return ValidateNaN(targetPosition + targetVelocity * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept;</para>
        /// <para>This is a faster calculation than SafeIntercept but does not handle situations where there is no solution such as a target moving away faster than the interceptor moves toward</para>
        /// </summary>
        /// <remarks>
        /// <para>Fast intercept is appropriate for projectile to target such as a bullet to a character or vehicle where the bullet is substantially faster than the subject
        /// however for other intercept cases where the interceptor is nearer the speed of the interceptee this method may result in frequent misses in such cases
        /// use the SafeIntercept and test for null result indicating no solution</para>
        /// </remarks>
        /// <param name="position">the point in space to start from</param>
        /// <param name="speed">the speed of travel</param>
        public static float3 FastIntercept(float3 position, float speed, float3 targetPosition, float3 targetHeading, float targetSpeed)
        {
            var distance = math.distance(position, targetPosition);
            return ValidateNaN(targetPosition + (targetHeading * targetSpeed) * (distance / speed));
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        /// <param name="position">The position of the interceptor</param>
        /// <param name="speed">The speed of the interceptor</param>
        /// <param name="targetSubject">The subject to be intercepted</param>
        /// <returns></returns>
        public static float3? SafeIntercept(float3 position, float speed, Rigidbody targetSubject)
        {
            float3? result = null;
            float3 tPos = targetSubject.position;
            float3 tVelocity = targetSubject.linearVelocity;
            float3 localPosition = tPos - position;
            float3 closingVelocity = tVelocity - math.normalize((position - tPos) * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if(t > 0)
            {
                result = ValidateNaN(targetSubject.position + targetSubject.linearVelocity * t);
            }

            return result;
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        public static float3? SafeIntercept(float3 position, float speed, float3 targetPosition, float3 targetVelocity)
        {
            float3? result = null;

            var localPosition = targetPosition - position;
            var closingVelocity = targetVelocity - math.normalize((position - targetPosition) * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if (t > 0)
            {
                result = ValidateNaN(targetPosition + targetVelocity * t);
            }

            return result;
        }

        /// <summary>
        /// <para>Find the point of intercept if any</para>
        /// <para>This is a slower calculation than FastIntercept but indicates when no intercept solution is available.
        /// Use this when the interceptor's speed is at or below the speed of the target</para>
        /// </summary>
        public static float3? SafeIntercept(float3 position, float speed, float3 targetPosition, float3 targetHeading, float targetSpeed)
        {
            float3? result = null;

            var localPosition = targetPosition - position;
            var closingVelocity = (targetHeading * targetSpeed) - (math.normalize(position - targetPosition) * speed);

            var t = InterceptTime(speed, localPosition, closingVelocity);
            if (t > 0)
            {
                result = ValidateNaN(targetPosition + (targetHeading * targetSpeed) * t);
            }

            return result;
        }

        /// <summary>
        /// Calculates the time to intercept given the local position e.g. relative position and the closing velocity e.g. relative velocity
        /// </summary>
        /// <param name="speed">The speed of interceptor</param>
        /// <param name="localPosition">The local or 'relative' position of the target to interceptor</param>
        /// <param name="closingVelocity">The closing or 'relative' velocity between the interceptor and target</param>
        /// <returns>The time to intercept ... if 0 then no valid intercept solution exists</returns>
        public static float InterceptTime(float speed, float3 localPosition, float3 closingVelocity)
        {
            var sqrMag = math.lengthsq(closingVelocity);

            //No solution
            if (sqrMag < 0.001f)
                return 0;

            var velDelta = sqrMag - speed * speed;

            //Handle similar speeds
            if(math.abs(velDelta) < 0.001f)
            {
                var t = -math.lengthsq(localPosition) / (2 * math.dot(closingVelocity, localPosition));
                return math.max(t, 0);
            }

            var quote = 2 * math.dot(closingVelocity, localPosition);
            var sqrPos = math.lengthsq(localPosition);
            var determinant = quote * quote - 4 * velDelta * sqrPos;

            if(determinant > 0)
            {
                //Two possible solutions
                var t1 = (-quote + math.sqrt(determinant)) / (2f * velDelta);
                var t2 = (-quote - math.sqrt(determinant)) / (2f * velDelta);

                if (t1 > 0)
                {
                    if (t2 > 0)
                        return Mathf.Min(t1, t2);
                    else
                        return t1;
                }
                else
                    return math.max(t2, 0);
            }
            else if (determinant < 0)
            {
                //No solution
                return 0;
            }
            else
            {
                //One possible solution
                return math.max(-quote / (2 * velDelta), 0);
            }
        }

        /// <summary>
        /// Find the effect of drag on an object in motion through a volume
        /// </summary>
        /// <param name="dragCoefficient">Use DragCoefficients helper for common values</param>
        /// <param name="fluidDensity">Use VolumetricMassDensity helper for common values</param>
        /// <param name="flowVelocity">Typically the inverse of the subject velocity</param>
        /// <param name="crossSectionArea">Typically the orthagraphicly projected surface or 'lead face' of the subject i.e. exposed surface area in the direction of movement</param>
        /// <returns></returns>
        public static float3 QuadraticDrag(float dragCoefficient, float fluidDensity, float3 flowVelocity, float crossSectionArea)
        {
            return dragCoefficient * crossSectionArea * 0.5f * fluidDensity * math.lengthsq(flowVelocity) * math.normalize(flowVelocity);
        }

        /// <summary>
        /// Find the drag on an object based on its speed through a volume
        /// </summary>
        public static float QuadraticDrag(float dragCoefficient, float fluidDensity, float speed, float crossSectionArea)
        {
            return dragCoefficient * crossSectionArea * 0.5f * fluidDensity * speed * speed;
        }

        /// <summary>
        /// Returns the force to be applied to achieve a given velocity
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public static float3 ForceToReachLinearVelocity(Rigidbody body, float3 targetVelocity)
        {
            float3 bVel = body.linearVelocity;
            return body.mass * ValidateNaN(((targetVelocity - bVel) / Time.fixedDeltaTime));
        }

        /// <summary>
        /// Returns the force to be applied to achieve a given angular velocity
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public static float3 ForceToReachAngularVelocity(Rigidbody body, float3 targetVelocity)
        {
            float3 bAVel = body.angularVelocity;
            return body.mass * ValidateNaN(((targetVelocity - bAVel) / Time.fixedDeltaTime));
        }

        /// <summary>
        /// Returns an impulse force to rotate a body to align forward with a given direction
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetDirection"></param>
        /// <returns></returns>
        public static float3 TorqueToReachDirection(Rigidbody body, float3 targetDirection)
        {
            var x = math.cross(body.transform.forward, math.normalize(targetDirection));
            float th = math.asin(math.length(x));
            var w = math.normalize(x) * th / Time.fixedDeltaTime;

            var q = body.rotation * body.inertiaTensorRotation;
            float3 bInertiaTensor = body.inertiaTensor;
            float3 bAngVel = body.angularVelocity;
            
            return body.mass * ValidateNaN(math.mul(q, math.mul(bInertiaTensor, math.mul(math.inverse(q), w))) - bAngVel);
        }

        /// <summary>
        /// Returns an impulse force to rotate a body to align forward with a given direction
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetDirection"></param>
        /// <returns></returns>
        public static float TorqueToReachDirection2D(Rigidbody2D body, float2 targetDirection)
        {
            var dirNorm = math.normalize(targetDirection);
            var x = math.cross(body.transform.right, new float3(dirNorm.x, dirNorm.y, 0));
            float th = math.asin(math.length(x));
            float w = th / Time.fixedDeltaTime;

            return (w - body.angularVelocity) * body.mass * body.angularDamping;
        }

        /// <summary>
        /// Returns an impulse force to rotate a body to align to a given rotation
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetDirection"></param>
        /// <returns></returns>
        public static float3 TorqueToReachRotation(Rigidbody body, quaternion targetRotation)
        {
            
            var targetDirection = math.mul(targetRotation, math.forward());
            return ValidateNaN(TorqueToReachDirection(body, targetDirection));
        }

        /// <summary>
        /// returns the intermediate position simulating constant movement at speed from position toward target
        /// </summary>
        /// <param name="position">The current position</param>
        /// <param name="targetPosition">The target position</param>
        /// <param name="speed">The current speed</param>
        /// <param name="deltaTime">The elapsed time this frame</param>
        /// <returns>The new position of the subject being moved</returns>
        public static float3 LerpTo(float3 position, float3 targetPosition, float speed, float deltaTime)
        {
            var tDistance = math.distance(position, targetPosition);
            var aDistance = speed * deltaTime;

            //If we are within range for this step
            if (aDistance > tDistance)
                return targetPosition;
            else //We wont be at target this step so lerp toward the target by the quota of aDistance and tDistance
                return ValidateNaN(math.lerp(position, targetPosition, aDistance / tDistance));
        }

        /// <summary>
        /// returns the intermediate rotation simulating constant rotation at speed from rotation toward target
        /// </summary>
        /// <param name="rotation">The current rotation</param>
        /// <param name="targetRotation">The target rotation</param>
        /// <param name="speed">The current speed</param>
        /// <param name="deltaTime">The elapsed time this frame</param>
        /// <returns>The new rotation of the subject being rotated</returns>
        public static Quaternion LerpTo(Quaternion rotation, Quaternion targetRotation, float speed, float deltaTime)
        {
            var tDistance = Quaternion.Angle(rotation, targetRotation);
            var aDistance = speed * deltaTime;

            if (aDistance > tDistance)
                return targetRotation;
            else
                return Quaternion.Lerp(rotation, targetRotation, aDistance / tDistance);
        }

        /// <summary>
        /// returns the force to apply to the rigidbody to accelerate toward the desired velocity at the indicated rate
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="targetVelocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float3 LerpTo(Rigidbody subject, float3 targetVelocity, float acceleration, float deltaTime)
        {
            var stepVelocity = LerpTo(subject.linearVelocity, targetVelocity, acceleration, deltaTime);
            return ForceToReachLinearVelocity(subject, stepVelocity);
        }

        /// <summary>
        /// returns the torque force to apply to the rigidbody to move toward the target rotation at the indicated speed
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="targetRotation"></param>
        /// <param name="speed"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float3 LerpTo(Rigidbody subject, Quaternion targetRotation, float speed, float deltaTime)
        {
            var stepRotation = LerpTo(subject.rotation, targetRotation, speed, deltaTime);
            return TorqueToReachRotation(subject, stepRotation);
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static float3 ValidateNaN(float3 value)
        {
            var val = new float3(
                float.IsNaN(value.x) ? 0f : value.x,
                float.IsNaN(value.y) ? 0f : value.y,
                float.IsNaN(value.z) ? 0f : value.z);

            return val;
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static float2 ValidateNaN(float2 value)
        {
            var val = new float2(
                float.IsNaN(value.x) ? 0f : value.x,
                float.IsNaN(value.y) ? 0f : value.y);

            return val;
        }

        /// <summary>
        /// Resolves NaN values to 0s
        /// </summary>
        public static float ValidateNaN(float value)
        {
            return float.IsNaN(value) ? 0f : value;
        }

        /// <summary>
        /// Rotates a vector around a vector by eular angles
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angles"></param>
        /// <returns></returns>
        public static float3 RotatePointAroundPivot(float3 point, float3 pivot, float3 angles)
        {
            float3 dir = point - pivot; 
            dir = Quaternion.Euler(angles) * dir;
            point = dir + pivot; 
            return point; 
        }

        /// <summary>
        /// Rotates a vector around a vector by the rotation provided
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <param name="angles"></param>
        /// <returns></returns>
        public static float3 RotatePointAroundPivot(float3 point, float3 pivot, Quaternion rotation)
        {
            float3 dir = point - pivot;
            dir = rotation * dir;
            point = dir + pivot;
            return point;
        }

        /// <summary>
        /// Returns a point on the line nearest the subject
        /// </summary>
        /// <param name="lineStart">A end of the line segment</param>
        /// <param name="lineEnd">A end of the line segment</param>
        /// <param name="subject">The point to test from</param>
        /// <returns></returns>
        public static float3 NearestPointOnLineSegment(float3 lineStart, float3 lineEnd, float3 subject)
        {
            var line = (lineEnd - lineStart);
            var length = math.length(line);
            line = math.normalize(line);

            var subjectHeading = subject - lineStart;
            var dot = math.dot(subjectHeading, line);
            dot = Mathf.Clamp(dot, 0f, length);
            return lineStart + line * dot;
        }

        /// <summary>
        /// Returns a point on a ray (line without end)
        /// </summary>
        /// <param name="lineStart"></param>
        /// <param name="lineDirection"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static float3 NearestPointOnLine(float3 lineStart, float3 lineDirection, float3 subject)
        {
            var direction = math.normalize(lineDirection);
            var subjectHeading = subject - lineStart;
            var dot = math.dot(subjectHeading, direction);
            return lineStart + direction * dot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        private static double GetCubicRoot(double value)
        {
            if (value > 0.0)
            {
                return System.Math.Pow(value, 1.0 / 3.0);
            }
            else if (value < 0)
            {
                return -System.Math.Pow(-value, 1.0 / 3.0);
            }
            else
            {
                return 0.0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveCubic(double c0, double c1, double c2, double c3, out double s0, out double s1, out double s2)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;

            int num;
            double sub;
            double A, B, C;
            double sq_A, p, q;
            double cb_p, D;

            /* normal form: x^3 + Ax^2 + Bx + C = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;

            /*  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0 */
            sq_A = A * A;
            p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
            q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

            /* use Cardano's formula */
            cb_p = p * p * p;
            D = q * q + cb_p;

            if (D == 0)
            {
                if (q == 0) /* one triple solution */
                {
                    s0 = 0;
                    num = 1;
                }
                else /* one single and one double solution */
                {
                    double u = GetCubicRoot(-q);
                    s0 = 2 * u;
                    s1 = -u;
                    num = 2;
                }
            }
            else if (D < 0) /* Casus irreducibilis: three real solutions */
            {
                double phi = 1.0 / 3 * System.Math.Acos(-q / System.Math.Sqrt(-cb_p));
                double t = 2 * System.Math.Sqrt(-p);

                s0 = t * System.Math.Cos(phi);
                s1 = -t * System.Math.Cos(phi + System.Math.PI / 3);
                s2 = -t * System.Math.Cos(phi - System.Math.PI / 3);
                num = 3;
            }
            else /* one real solution */
            {
                double sqrt_D = System.Math.Sqrt(D);
                double u = GetCubicRoot(sqrt_D - q);
                double v = -GetCubicRoot(sqrt_D + q);

                s0 = u + v;
                num = 1;
            }

            /* resubstitute */
            sub = 1.0 / 3 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;

            return num;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveQuadric(double c0, double c1, double c2, out double s0, out double s1)
        {
            s0 = double.NaN;
            s1 = double.NaN;

            double p, q, D;

            /* normal form: x^2 + px + q = 0 */
            p = c1 / (2 * c0);
            q = c2 / c0;

            D = p * p - q;

            if (D == 0)
            {
                s0 = -p;
                return 1;
            }
            else if (D < 0)
            {
                return 0;
            }
            else /* if (D > 0) */
            {
                double sqrt_D = System.Math.Sqrt(D);

                s0 = sqrt_D - p;
                s1 = -sqrt_D - p;
                return 2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Ported from GraphicsGems by Jochen Schwarze
        /// </remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int SolveQuartic(double c0, double c1, double c2, double c3, double c4, out double s0, out double s1, out double s2, out double s3)
        {
            s0 = double.NaN;
            s1 = double.NaN;
            s2 = double.NaN;
            s3 = double.NaN;

            double[] coeffs = new double[4];
            double z, u, v, sub;
            double A, B, C, D;
            double sq_A, p, q, r;
            int num;

            /* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */
            A = c1 / c0;
            B = c2 / c0;
            C = c3 / c0;
            D = c4 / c0;

            /*  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0 */
            sq_A = A * A;
            p = -3.0 / 8 * sq_A + B;
            q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
            r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

            if (r == 0)
            {
                /* no absolute term: y(y^3 + py + q) = 0 */

                coeffs[3] = q;
                coeffs[2] = p;
                coeffs[1] = 0;
                coeffs[0] = 1;

                num = SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);
            }
            else
            {
                /* solve the resolvent cubic ... */
                coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
                coeffs[2] = -r;
                coeffs[1] = -1.0 / 2 * p;
                coeffs[0] = 1;

                SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);

                /* ... and take the one real solution ... */
                z = s0;

                /* ... to build two quadric equations */
                u = z * z - r;
                v = 2 * z - p;

                if (u == 0)
                    u = 0;
                else if (u > 0)
                    u = System.Math.Sqrt(u);
                else
                    return 0;

                if (v == 0)
                    v = 0;
                else if (v > 0)
                    v = System.Math.Sqrt(v);
                else
                    return 0;

                coeffs[2] = z - u;
                coeffs[1] = q < 0 ? -v : v;
                coeffs[0] = 1;

                num = SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);

                coeffs[2] = z + u;
                coeffs[1] = q < 0 ? v : -v;
                coeffs[0] = 1;

                if (num == 0) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);
                else if (num == 1) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s1, out s2);
                else if (num == 2) num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s2, out s3);
            }

            /* resubstitute */
            sub = 1.0 / 4 * A;

            if (num > 0) s0 -= sub;
            if (num > 1) s1 -= sub;
            if (num > 2) s2 -= sub;
            if (num > 3) s3 -= sub;

            return num;
        }

        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetForwardDirection(Quaternion rotation) => rotation * math.forward();
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetBackDirection(Quaternion rotation) => rotation * math.back();
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetUpDirection(Quaternion rotation) => rotation * math.up();
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetDownDirection(Quaternion rotation) => rotation * math.down();
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetRightDirection(Quaternion rotation) => rotation * math.right();
        /// <summary>
        /// How do I get the X direction vector from quaternion rotaiton
        /// </summary>
        /// <remarks>
        /// What we assume you are asking is if I was standing up facing forward and rotated by this quaternion what direction would my X direction vector be pointing.
        /// The formula is simply direction = rotaiton * normal ... so for forward it would be imFacing = rotation * float3.forward;
        /// </remarks>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 GetLeftDirection(Quaternion rotation) => rotation * math.left();
        /// <summary>
        /// How do I convert a direction vector to a rotaiton
        /// </summary>
        /// <remarks>
        /// We assume by this you mean if I wanted to rotate to look down this direction what would that rotaiton be
        /// Unity provides for this as well x = Quaternion.LookRotation(direction);
        /// </remarks>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Quaternion GetForwardDirection(float3 direction) => Quaternion.LookRotation(direction);

        /// <summary>
        /// The distance an object will have fallen given an input gravity and amount of time. 
        /// This is useful for various calcualtions.
        /// </summary>
        /// <param name="gravity"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float FallDistance(float gravity, float time) => 0.5f * gravity * time * time;

        public static float3 SimulateWindForce(float main, float frequency, float magnitude, float turbulence, float3 direction, float gameTime)
        {
            float3 d = direction;
            if (turbulence > 0)
            {
                
                var range = Mathf.LerpAngle(-90f, 90f, noise.cnoise(new float2(gameTime * turbulence, 1f)));
                d = math.normalize(Quaternion.AngleAxis(range, math.up()) * d);
            }
            var f = d * main;
            var p = d * math.sin(gameTime * frequency * 90) * magnitude;

            return f + p;
        }

        /// <summary>
        /// Retrurns the size in world units at a given distance from the screen
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static float ScreenSizeAtDistance(Camera camera, float distance)
        {
            if (camera.orthographic)
                return camera.orthographicSize * 2;
            else
                return 2f * distance * math.tan(math.radians(camera.fieldOfView) * 0.5f);
        }

        /// <summary>
        /// Finds the scale an object should take to have the same size on screen at the testDistance as it has at the natural distnace
        /// </summary>
        /// <param name="camera">The camera to test for</param>
        /// <param name="naturalDistance">The distance at which the object has a scale of 1 and is the desired size on screen. This is typically the "starting" distance for an object you want to appear to remain the same size on screen</param>
        /// <param name="testDistance">The distance at which the object is now</param>
        /// <returns></returns>
        public static float ScalarByDistance(Camera camera, float naturalDistance, float testDistance)
        {
            var natural = ScreenSizeAtDistance(camera, naturalDistance);
            var test = ScreenSizeAtDistance(camera, testDistance);
            return test / natural;
        }

        public static bool CircleContains(float2 center, float radius, float2 point)
        {
            return math.distance(center, point) <= radius;
        }

        public static bool SphereContains(float3 center, float radius, float3 point)
        {
            return math.distance(center, point) <= radius;
        }

        public static bool EllipseContains(float2 center, float xRadius, float yRadius, float2 point)
        {
            var local = point - center;
            return ((local.x * local.x) / (xRadius * xRadius)) + ((local.y * local.y) / (yRadius * yRadius)) <= 1f;
        }

        public static bool EllipsoidContains(float3 center, float xRadius, float yRadius, float zRadius, float3 point)
        {
            var local = point - center;
            return ((local.x * local.x) / (xRadius * xRadius)) + ((local.y * local.y) / (yRadius * yRadius)) + ((local.z * local.z) / (zRadius * zRadius)) <= 1f;
        }

        public static float2 GetPointOnCircle(float2 center, float radius, float angle)
        {
            return new float2(center.x + radius * math.sin(math.radians(angle)), center.y + radius * math.cos(math.radians(angle)));
        }

        public static float2 GetPointOnEllipse(float2 center, float xRadius, float yRadius, float angle)
        {
            return new float2(center.x + xRadius * math.sin(math.radians(angle)), center.y + yRadius * math.cos(math.radians(angle)));
        }

        /// <summary>
        /// A simplified Verlet integration correcting for variable time.
        /// Calculates the new position of an object using Velocity Verlet integration method with variable time steps.
        /// </summary>
        /// <param name="currentPosition">The current world position of the particle</param>
        /// <param name="priorPosition">The previous world position of the particle</param>
        /// <param name="inertia">A value between 0 and 1 that represents how much of the existing velocity should be applied to the next state. avoid the value of 1 or higher as this will result in an unstable system.</param>
        /// <param name="acceleration">The sum of all acceleration effecting this particle this frame, this is typically expressed as meters per second e.g. 0,-9.81,0 would be gravity.</param>
        /// <param name="scale">The amount of acceleration to be applied to the next position ... sometimes called "Dampen"</param>
        /// <param name="currentTimestep">The current "delta" time</param>
        /// <param name="priorTimestep">The prior "delta" time</param>
        public static float3 TimeCorrectedVerletIntegration(float3 currentPosition, float3 priorPosition, float inertia, float3 acceleration, float scale, float currentTimestep, float priorTimestep)
        {
            var inertialVelocity = (currentPosition - priorPosition) * inertia;
            return currentPosition + inertialVelocity * currentTimestep / priorTimestep + (acceleration * scale) * currentTimestep * (currentTimestep + priorTimestep) / 2f;
        }

        /// <summary>
        /// A simplified Verlet integration correcting for variable time.
        /// Calculates the new position of an object using Velocity Verlet integration method with variable time steps.
        /// </summary>
        /// <param name="currentPosition">The current world position of the particle</param>
        /// <param name="priorPosition">The previous world position of the particle</param>
        /// <param name="inertia">A value between 0 and 1 that represents how much of the existing velocity should be applied to the next state. avoid teh value of 1 or higher as this will result in an unstable system.</param>
        /// <param name="acceleration">The sum of all acceleration effecting this particle this frame, this is typically expressed as meters per second e.g. 0,-9.81,0 would be gravity.</param>
        /// <param name="scale">The amount of acceleration to be applied to the next position ... sometimes called "Dampen"</param>
        /// <param name="currentTimestep">The current "delta" time</param>
        /// <param name="priorTimestep">The prior "delta" time</param>
        public static float2 TimeCorrectedVerletIntegration2D(float2 currentPosition, float2 priorPosition, float inertia, float2 acceleration, float scale, float currentTimestep, float priorTimestep)
        {
            var inertialVelocity = (currentPosition - priorPosition) * inertia;
            return currentPosition + inertialVelocity * currentTimestep / priorTimestep + (acceleration * scale) * currentTimestep * (currentTimestep + priorTimestep) / 2f;
        }
        /// <summary>
        /// Find the acceleration applied to a subject given force and mass
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="force"></param>
        /// <returns>The applied acceleration</returns>
        public static float3 Acceleration(float mass, float3 force) => force / mass;
        /// <summary>
        /// Find the acceleration applied to a subject given force and mass
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="force"></param>
        /// <returns>The applied acceleration</returns>
        public static float2 Acceleration2D(float mass, float2 force) => force / mass;
        /// <summary>
        /// Find the acceleration applied to a subject given displacement, elasticity and mass
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="displacement"></param>
        /// <param name="elasticity"></param>
        /// <returns>The applied acceleration</returns>
        public static float3 ElasticityAcceleration(float mass, float3 displacement, float elasticity) => displacement * elasticity / mass;
        /// <summary>
        /// Find the acceleration applied to a subject given displacement, elasticity and mass
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="displacement"></param>
        /// <param name="elasticity"></param>
        /// <returns>The applied acceleration</returns>
        public static float2 ElasticityAcceleration2D(float mass, float2 displacement, float elasticity) => displacement * elasticity / mass;
        /// <summary>
        /// Find the torque applied to a pivot point given a force at a position
        /// </summary>
        /// <param name="force"></param>
        /// <param name="position"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static float3 Torque(float3 force, float3 position, float3 pivot) => math.cross(position - pivot, force);
        /// <summary>
        /// Find the torque applied to a pivot point given a force at a position
        /// </summary>
        /// <param name="force"></param>
        /// <param name="position"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static float Torque2D(float2 force, float2 position, float2 pivot)
        {
            var leverArm = position - pivot;
            return (leverArm.x * force.y) - (leverArm.y * force.x);   
        }
        /// <summary>
        /// Find the rotational difference between two rotations
        /// </summary>
        /// <param name="fromRotation"></param>
        /// <param name="toRotation"></param>
        /// <returns>The rotation that if applied to from the result would be to</returns>
        public static quaternion QuaternionDifference(quaternion fromRotation, quaternion toRotation)
        {
            quaternion diff = math.mul(toRotation, math.inverse(fromRotation));
            return diff;
        }
        /// <summary>
        /// Find the rotation if a quantity rotation was applied to a from rotation
        /// </summary>
        /// <param name="fromRotation"></param>
        /// <param name="quantityRotation"></param>
        /// <returns>The resulting rotation if quantity was applied to from</returns>
        public static quaternion QuaternionSum(quaternion fromRotation, quaternion quantityRotation)
        {
            return math.mul(quantityRotation, fromRotation);
        }
    }
}

#endif