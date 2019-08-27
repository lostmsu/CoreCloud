using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RayTracer.Geometry;
using RayTracer.Scenes;
using RayTracer.Material;

namespace RayTracer {
    public class Tracer {

        private int screenWidth;
        private int screenHeight;
        public const int DefaultDepth = 5;

        public Action<int, int, System.Drawing.Color> setPixel;

        public Tracer(int screenWidth, int screenHeight, Action<int,int, System.Drawing.Color> setPixel) {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.setPixel = setPixel;
        }

        static IEnumerable<Intersection> Intersections(Ray ray, Scene scene)
        {
            return scene.Things
                        .Select(obj => obj.Intersect(ray));
        }

		static Intersection Intersect(Ray ray, Scene scene)
		{
			var isect = Intersection.Empty;
			foreach (var obj in scene.Things) {
				var curr = obj.Intersect(ray);
				isect = curr.Distance < isect.Distance ? curr : isect;
			}
			return isect;
		}

        static double TestRay(Ray ray, Scene scene) {
			var isect = Intersect(ray, scene);
			return isect.Intersects ? isect.Distance : 0;
        }

        public static Color TraceRay(Ray ray, Scene scene, int depth = DefaultDepth) {
			var isect = Intersect(ray, scene);
			return isect.Intersects
				? Shade(isect, scene, depth)
				: Color.Background;
        }

        static Color GetNaturalColor(ISceneObject thing, Vector pos, Vector norm, Vector rd, Scene scene) {
            Color ret = Color.Black;
            foreach (Light light in scene.Lights) {
                Vector ldis = light.Pos - pos;
				Vector livec = ldis.Normalize();
                double neatIsect = TestRay(new Ray{ Refraction = 1, Start = pos, Dir = livec }, scene);
                bool isInShadow = !((neatIsect > ldis.GetLength()) || (neatIsect == 0));
                if (!isInShadow) {
                    double illum = livec.DotProduct(norm);
                    Color lcolor = illum > 0 ? illum * light.Color : Color.Black;
                    double specular = rd.Normalize().DotProduct(livec);
                    Color scolor = specular > 0 ? Math.Pow(specular, thing.Surface.Roughness) * light.Color : Color.Black;
					var power = 4 / ldis.DotProduct(ldis);
                    ret += power * (thing.Surface.Diffuse(pos) * lcolor + thing.Surface.Specular(pos) * scolor);
                }
            }
            return ret;
        }

		static Color GetReflectionColor(Surface surf, Vector pos, Vector norm, Vector rd, Scene scene, int depth)
		{
			return surf.Reflect(pos) * TraceRay(new Ray { Refraction = 1, Start = pos, Dir = rd }, scene, depth - 1);
        }

		static Color GetRefractionColor(Surface surf, Vector pos, Vector norm, Vector rd, double refr, Scene scene, int depth)
		{
			var ray = new Ray { Refraction = refr, Start = pos + 0.001 * rd, Dir = rd };
			return surf.Reflect(pos) * TraceRay(ray, scene, depth - 1);
		}

        static Color Shade(Intersection isect, Scene scene, int depth) {
            var d = isect.Ray.Dir;
            var pos = isect.Distance * isect.Ray.Dir + isect.Ray.Start;
            var normal = isect.Thing.Normal(pos);
			var c1 = normal.DotProduct(d);
            var reflectDir = d - 2 * c1 * normal;

			var refIndex = isect.Thing.Surface.RefractionIndex(pos);
			if (Math.Abs(refIndex - isect.Ray.Refraction) < 0.00001) refIndex = 1;
			var n = isect.Ray.Refraction / refIndex;
			var c2 = 1 - n * n * (1 - c1 * c1);
			bool refract = c2 > 0;
			var refractDir = refract? (n * d - (n * c1 - Math.Sqrt(c2)) * normal) :Vector.Empty;
            var ret = GetNaturalColor(isect.Thing, pos, normal, reflectDir, scene);
            if (depth <= 0) {
                return ret + Color.Grey;
            }
			var surf = isect.Thing.Surface;
			var refractPercent = refract? surf.RefractedPercent(pos): 0;
            return ret + (1 - refractPercent) * GetReflectionColor(surf, pos + .001 * reflectDir, normal, reflectDir, scene, depth) 
				+ (refract? refractPercent * GetRefractionColor(surf, pos, normal, refractDir, refIndex, scene, depth): Color.Black);
        }

        static double RecenterX(double x, int screenWidth) {
            return (x - (screenWidth / 2.0)) / (2.0 * screenWidth);
        }
        static double RecenterY(double y, int screenHeight) {
            return -(y - (screenHeight / 2.0)) / (2.0 * screenHeight);
        }

        private Vector GetPoint(double x, double y, Camera camera) {
            return (camera.Forward + RecenterX(x, screenWidth) * camera.Right +
                                     RecenterY(y, screenHeight) * camera.Up)
					.Normalize();
        }

        internal void Render(Scene scene) {
            for (int y = 0; y < screenHeight; y++)
            {
				if (LineRendered != null) LineRendered(this, EventArgs.Empty);

                for (int x = 0; x < screenWidth; x++)
                {
                    Color color = TraceRay(new Ray { Refraction = 1, Start = scene.Camera.Pos, Dir = GetPoint(x, y, scene.Camera) }, scene, 0);
                    setPixel(x, y, color.ToDrawingColor());
                }
            }
        }

		public static double Recenter(double value, double dimension)
		{
			return (value - (dimension / 2)) / (2 * dimension);
		}

		public static void Render(Scene scene, Color[,] target, int line, int maxDepth = DefaultDepth)
		{
			if (target == null) throw new ArgumentNullException("target");

			int h = target.GetLength(0);
			int w = target.GetLength(1);

			for (int x = 0; x < w; x++) {
				var rx = RecenterX(x, w); var ry = RecenterY(line, h);
				target[line, x] = TraceRay(scene, rx, ry, maxDepth);
			}
		}

		public static void Render(Scene scene, Color[,] target, Action<int> progress = null, int maxDepth = DefaultDepth)
		{
			if (target == null) throw new ArgumentNullException("target");

			int h = target.GetLength(0);
			int w = target.GetLength(1);

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					var rx = RecenterX(x, w); var ry = RecenterY(y, h);
					target[y, x] = TraceRay(scene, rx, ry, maxDepth);
				}

				if (progress != null) progress(y);
			}
		}

		public static Color[,] Render(Scene scene, int width, int height, int maxDepth = DefaultDepth, Action<int> online = null)
		{
			var result = new Color[height, width];
			Render(scene, result, online, maxDepth);
			return result;
		}

		public static Color TraceRay(Scene scene, double x, double y, int depth)
		{
			var dir = (scene.Camera.Forward + x * scene.Camera.Right + y * scene.Camera.Up).Normalize();
			var ray = new Ray { Refraction = 1, Start = scene.Camera.Pos, Dir = dir };
			return TraceRay(ray, scene, depth);
		}

		public EventHandler LineRendered;

        public static readonly Scene DefaultScene =
            new Scene() {
                    Things = new ISceneObject[] { 
                                new Plane() {
                                    Norm = Vector.Make(0,1,0),
                                    Offset = 0,
                                    Surface = Surfaces.CheckerBoard
                                },
                                new Sphere() {
                                    Center = Vector.Make(0,1,0),
                                    Radius = 1,
                                    Surface = Surfaces.Shiny
                                },
                                new Sphere() {
                                    Center = Vector.Make(-1,.5,1.5),
                                    Radius = .5,
                                    Surface = Surfaces.Shiny
                                }},
                    Lights = new Light[] { 
                                new Light() {
                                    Pos = Vector.Make(-2,2.5,0),
                                    Color = Color.Make(.49,.07,.07)
                                },
                                new Light() {
                                    Pos = Vector.Make(1.5,2.5,1.5),
                                    Color = Color.Make(.07,.07,.49)
                                },
                                new Light() {
                                    Pos = Vector.Make(1.5,2.5,-1.5),
                                    Color = Color.Make(.07,.49,.071)
                                },
                                new Light() {
                                    Pos = Vector.Make(0,3.5,0),
                                    Color = Color.Make(.21,.21,.35)
                                }},
                    Camera = Camera.Create(Vector.Make(3,2,4), Vector.Make(-1,.5,0))
                };
    }
}
