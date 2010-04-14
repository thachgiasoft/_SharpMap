// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMap.Geometries
{
	/// <summary>
	/// A Polygon is a planar Surface, defined by 1 exterior boundary and 0 or more interior boundaries. Each
	/// interior boundary defines a hole in the Polygon.
    /// </summary>
    public class Polygon : Surface
	{
		private LinearRing _ExteriorRing;
		private List<LinearRing> _InteriorRings;

		/// <summary>
		/// Instatiates a polygon based on one extorier ring and a collection of interior rings.
		/// </summary>
		/// <param name="exteriorRing">Exterior ring</param>
		/// <param name="interiorRings">Interior rings</param>
		public Polygon(LinearRing exteriorRing, List<LinearRing> interiorRings)
		{
			_ExteriorRing = exteriorRing;
			_InteriorRings = interiorRings;
		}

		/// <summary>
		/// Instatiates a polygon based on one extorier ring.
		/// </summary>
		/// <param name="exteriorRing">Exterior ring</param>
		public Polygon(LinearRing exteriorRing) : this(exteriorRing, new List<LinearRing>()) { }

		/// <summary>
		/// Instatiates a polygon
		/// </summary>
		public Polygon() : this(new LinearRing(), new List<LinearRing>()) { }

		/// <summary>
		/// Gets or sets the exterior ring of this Polygon
		/// </summary>
		/// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
		public LinearRing ExteriorRing
		{
			get { return _ExteriorRing; }
			set { _ExteriorRing = value; }
		}

		/// <summary>
		/// Gets or sets the interior rings of this Polygon
		/// </summary>
		public List<LinearRing> InteriorRings
		{
			get { return _InteriorRings; }
			set { _InteriorRings = value; }
		}

		/// <summary>
		/// Returns the Nth interior ring for this Polygon as a LineString
		/// </summary>
		/// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
		/// <param name="N"></param>
		/// <returns></returns>
		public LinearRing InteriorRing(int N)
		{
			return _InteriorRings[N];
		}

		/// <summary>
		/// Returns the number of interior rings in this Polygon
		/// </summary>
		/// <remarks>This method is supplied as part of the OpenGIS Simple Features Specification</remarks>
		/// <returns></returns>
		public int NumInteriorRing()
		{
			return _InteriorRings.Count;
        }

		/// <summary>
		/// Transforms the polygon to image coordinates, based on the map
		/// </summary>
		/// <param name="map">Map to base coordinates on</param>
		/// <returns>Polygon in image coordinates</returns>
		public Point[] WorldToMap(IMapTransform transform)
		{

			int vertices = _ExteriorRing.Vertices.Count;
			for (int i = 0; i < _InteriorRings.Count;i++)
				vertices += _InteriorRings[i].Vertices.Count;

			Point[] v = new Point[vertices];
			for (int i = 0; i < _ExteriorRing.Vertices.Count; i++)
				v[i] = transform.WorldToMap(_ExteriorRing.Vertices[i]);
			int j = _ExteriorRing.Vertices.Count;
			for (int k = 0; k < _InteriorRings.Count;k++)
			{
				//The vertices of an interior polygon must be ordered counterclockwise to render as a hole
				//Uncomment the following lines if you are not sure of the orientation of your interior polygons
				//if (_InteriorRings[k].IsCCW())
				//{
					for (int i = 0; i < _InteriorRings[k].Vertices.Count; i++)
						v[j + i] = transform.WorldToMap(_InteriorRings[k].Vertices[i]);
				//}
				//else
				//{
				//	for (int i = 1; i <= _InteriorRings[k].Vertices.Count; i++)
				//		v[j + i] = SharpMap.Utilities.Transform.WorldtoMap(_InteriorRings[k].Vertices[_InteriorRings[k].Vertices.Count - i - 1], map);
				//}
				j += _InteriorRings[k].Vertices.Count;
			}
			return v;
		}



#region "Inherited methods from abstract class Geometry"

		/// <summary>
		/// Determines if this Polygon and the specified Polygon object has the same values
		/// </summary>
		/// <param name="p">Polygon to compare with</param>
		/// <returns></returns>
		public bool Equals(Polygon p)
		{
			if (!p.ExteriorRing.Equals(this.ExteriorRing))
				return false;
			if (p.InteriorRings.Count != this.InteriorRings.Count)
				return false;
				for (int i = 0; i < p.InteriorRings.Count; i++)
				if (!p.InteriorRings[i].Equals(this.InteriorRings[i]))
					return false;
			return true;
		}

		/// <summary>
		/// If true, then this Geometry represents the empty point set, �, for the coordinate space. 
		/// </summary>
		/// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
		public override bool IsEmpty()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///  Returns 'true' if this Geometry has no anomalous geometric points, such as self
		/// intersection or self tangency. The description of each instantiable geometric class will include the specific
		/// conditions that cause an instance of that class to be classified as not simple.
		/// </summary>
		public override bool IsSimple()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the closure of the combinatorial boundary of this Geometry. The
		/// combinatorial boundary is defined as described in section 3.12.3.2 of [1]. Because the result of this function
		/// is a closure, and hence topologically closed, the resulting boundary can be represented using
		/// representational geometry primitives
		/// </summary>
		public override Geometry Boundary()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the shortest distance between any two points in the two geometries
		/// as calculated in the spatial reference system of this Geometry.
		/// </summary>
		public override double Distance(Geometry geom)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a geometry that represents all points whose distance from this Geometry
		/// is less than or equal to distance. Calculations are in the Spatial Reference
		/// System of this Geometry.
		/// </summary>
		public override Geometry Buffer(double d)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Geometry�Returns a geometry that represents the convex hull of this Geometry.
		/// </summary>
		public override Geometry ConvexHull()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a geometry that represents the point set intersection of this Geometry
		/// with anotherGeometry.
		/// </summary>
		public override Geometry Intersection(Geometry geom)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a geometry that represents the point set union of this Geometry with anotherGeometry.
		/// </summary>
		public override Geometry Union(Geometry geom)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a geometry that represents the point set difference of this Geometry with anotherGeometry.
		/// </summary>
		public override Geometry Difference(Geometry geom)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a geometry that represents the point set symmetric difference of this Geometry with anotherGeometry.
		/// </summary>
		public override Geometry SymDifference(Geometry geom)
		{
			throw new NotImplementedException();
		}

#endregion

		/// <summary>
		/// The area of this Surface, as measured in the spatial reference system of this Surface.
		/// </summary>
		public override double Area
		{
			get
            {
                double area = 0.0;
                area += this._ExteriorRing.Area;
				for (int i = 0; i < _InteriorRings.Count;i++ )
					area -= _InteriorRings[i].Area;
                return area;
			}
		}

		/// <summary>
		/// The mathematical centroid for this Surface as a Point.
		/// The result is not guaranteed to be on this Surface.
		/// </summary>
		public override Point Centroid
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// A point guaranteed to be on this Surface.
		/// </summary>
		public override Point PointOnSurface
		{
			get { throw new NotImplementedException(); }
		}
		/// <summary>
		/// Returns the bounding box of the object
		/// </summary>
		/// <returns>bounding box</returns>
		public override BoundingBox GetBoundingBox()
		{
			if (_ExteriorRing.Vertices.Count == 0) return null;
			BoundingBox bbox = new BoundingBox(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);
			for (int i = 0; i < _ExteriorRing.Vertices.Count; i++)
			{
				bbox.Min.X = Math.Min(_ExteriorRing.Vertices[i].X, bbox.Min.X);
				bbox.Min.Y = Math.Min(_ExteriorRing.Vertices[i].Y, bbox.Min.Y);
				bbox.Max.X = Math.Max(_ExteriorRing.Vertices[i].X, bbox.Max.X);
				bbox.Max.Y = Math.Max(_ExteriorRing.Vertices[i].Y, bbox.Max.Y);
			}
			return bbox;
		}

		#region ICloneable Members

		/// <summary>
		/// Return a copy of this geometry
		/// </summary>
		/// <returns>Copy of Geometry</returns>
		public new Polygon Clone()
		{
			Polygon p = new Polygon();
			p.ExteriorRing = (LinearRing) this._ExteriorRing.Clone();
			for(int i=0;i<_InteriorRings.Count;i++)
				p.InteriorRings.Add(_InteriorRings[i].Clone() as LinearRing);
			return p;
		}

		#endregion

	}
}