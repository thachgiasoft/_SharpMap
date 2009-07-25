// Copyright 2007: Christian Graefe
// Copyright 2008: Dan Brecht and Joel Wilson
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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OSGeo.GDAL;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.Data;
using SharpMap.Geometries;
using Point=System.Drawing.Point;

namespace SharpMap.Layers
{
    /// <summary>
    /// Gdal raster image layer
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code lang="C#">
    /// myMap = new SharpMap.Map(new System.Drawing.Size(500,250);
    /// SharpMap.Layers.GdalRasterLayer layGdal = new SharpMap.Layers.GdalRasterLayer("Blue Marble", @"C:\data\bluemarble.ecw");
    /// myMap.Layers.Add(layGdal);
    /// myMap.ZoomToExtents();
    /// </code>
    /// </example>
    /// </remarks>
    public class GdalRasterLayer : Layer, IDisposable
    {
        static GdalRasterLayer()
        {
            FwToolsHelper.Configure();
        }
        private int _bitDepth = 8;
        private bool _colorCorrect = true; // apply color correction values
        private List<int[]> _curveLut;
        private bool _displayCIR;
        private bool _displayIR;
        protected BoundingBox _envelope;
        private double[] _gain = {1, 1, 1, 1};
        private double _gamma = 1;
        protected Dataset _gdalDataset;
        internal GeoTransform _geoTransform;
        private bool _haveSpot; // spot correction
        private Rectangle _histoBounds;
        private double _histoBrightness, _histoContrast;
        private List<int[]> _histogram; // histogram of image
        private double[] _histoMean;
        protected Size _imagesize;

        private double _innerSpotRadius;
        // outer radius is feather between inner radius and rest of image

        internal int _lbands;

        private List<int[]> _nonSpotCurveLut;
        private double[] _nonSpotGain = {1, 1, 1, 1};
        private double _nonSpotGamma = 1;

        private double _outerSpotRadius;
        private string _projectionWkt = "";
        private bool _showClip;
        // outer radius is feather between inner radius and rest of image

        private PointF _spot = new PointF(0, 0);
        private List<int[]> _spotCurveLut;
        private double[] _spotGain = {1, 1, 1, 1};

        private double _spotGamma = 1;
        private Point _stretchPoint;
        protected ICoordinateTransformation _transform;
        private Color _transparentColor = Color.Empty; // color in image to make transparent (i.e. for black fill)
        protected bool _useRotation = true; // use geographic information

        #region accessors

        private string _Filename;

        /// <summary>
        ///  Gets the version of fwTools that was used to compile and test this GdalRasterLayer
        /// </summary>
        public static string FWToolsVersion
        {
            get { return FwToolsHelper.FwToolsVersion; }
        }

        /// <summary>
        /// Gets or sets the filename of the raster file
        /// </summary>
        public string Filename
        {
            get { return _Filename; }
            set { _Filename = value; }
        }

        /// <summary>
        /// Gets or sets the bit depth of the raster file
        /// </summary>
        public int BitDepth
        {
            get { return _bitDepth; }
            set { _bitDepth = value; }
        }

        /// <summary>
        /// Gets or set the projection of the raster file
        /// </summary>
        public string Projection
        {
            get { return _projectionWkt; }
            set { _projectionWkt = value; }
        }

        /// <summary>
        /// Gets or sets to display IR Band
        /// </summary>
        public bool DisplayIR
        {
            get { return _displayIR; }
            set { _displayIR = value; }
        }

        /// <summary>
        /// Gets or sets to display color InfraRed
        /// </summary>
        public bool DisplayCIR
        {
            get { return _displayCIR; }
            set { _displayCIR = value; }
        }

        /// <summary>
        /// Gets or sets to display clip
        /// </summary>
        public bool ShowClip
        {
            get { return _showClip; }
            set { _showClip = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma
        /// </summary>
        public double Gamma
        {
            get { return _gamma; }
            set { _gamma = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma for Spot spot
        /// </summary>
        public double SpotGamma
        {
            get { return _spotGamma; }
            set { _spotGamma = value; }
        }

        /// <summary>
        /// Gets or sets to display gamma for NonSpot
        /// </summary>
        public double NonSpotGamma
        {
            get { return _nonSpotGamma; }
            set { _nonSpotGamma = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain
        /// </summary>
        public double[] Gain
        {
            get { return _gain; }
            set { _gain = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] SpotGain
        {
            get { return _spotGain; }
            set { _spotGain = value; }
        }

        /// <summary>
        /// Gets or sets to display red Gain for Spot spot
        /// </summary>
        public double[] NonSpotGain
        {
            get { return _nonSpotGain; }
            set { _nonSpotGain = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut
        /// </summary>
        public List<int[]> CurveLut
        {
            get { return _curveLut; }
            set { _curveLut = value; }
        }

        /// <summary>
        /// Correct Spot spot
        /// </summary>
        public bool HaveSpot
        {
            get { return _haveSpot; }
            set { _haveSpot = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut for Spot spot
        /// </summary>
        public List<int[]> SpotCurveLut
        {
            get { return _spotCurveLut; }
            set { _spotCurveLut = value; }
        }

        /// <summary>
        /// Gets or sets to display curve lut for NonSpot
        /// </summary>
        public List<int[]> NonSpotCurveLut
        {
            get { return _nonSpotCurveLut; }
            set { _nonSpotCurveLut = value; }
        }

        /// <summary>
        /// Gets or sets the center point of the Spot spot
        /// </summary>
        public PointF SpotPoint
        {
            get { return _spot; }
            set { _spot = value; }
        }

        /// <summary>
        /// Gets or sets the inner radius for the spot
        /// </summary>
        public double InnerSpotRadius
        {
            get { return _innerSpotRadius; }
            set { _innerSpotRadius = value; }
        }

        /// <summary>
        /// Gets or sets the outer radius for the spot (feather zone)
        /// </summary>
        public double OuterSpotRadius
        {
            get { return _outerSpotRadius; }
            set { _outerSpotRadius = value; }
        }

        /// <summary>
        /// Gets the true histogram
        /// </summary>
        public List<int[]> Histogram
        {
            get { return _histogram; }
        }

        /// <summary>
        /// Gets the quick histogram mean
        /// </summary>
        public double[] HistoMean
        {
            get { return _histoMean; }
        }

        /// <summary>
        /// Gets the quick histogram brightness
        /// </summary>
        public double HistoBrightness
        {
            get { return _histoBrightness; }
        }

        /// <summary>
        /// Gets the quick histogram contrast
        /// </summary>
        public double HistoContrast
        {
            get { return _histoContrast; }
        }

        /// <summary>
        /// Gets the number of bands
        /// </summary>
        public int Bands
        {
            get { return _lbands; }
        }

        /// <summary>
        /// Gets the GSD (Horizontal)
        /// </summary>
        public double GSD
        {
            get { return _geoTransform.HorizontalPixelResolution; }
        }

        ///<summary>
        /// Use rotation information
        /// </summary>
        public bool UseRotation
        {
            get { return _useRotation; }
            set
            {
                _useRotation = value;
                _envelope = GetExtent();
            }
        }

        public Size Size
        {
            get { return _imagesize; }
        }

        public bool ColorCorrect
        {
            get { return _colorCorrect; }
            set { _colorCorrect = value; }
        }

        public Rectangle HistoBounds
        {
            get { return _histoBounds; }
            set { _histoBounds = value; }
        }

        public ICoordinateTransformation Transform
        {
            get { return _transform; }
        }

        public Color TransparentColor
        {
            get { return _transparentColor; }
            set { _transparentColor = value; }
        }

        public Point StretchPoint
        {
            get
            {
                if (_stretchPoint.Y == 0)
                    ComputeStretch();

                return _stretchPoint;
            }
            set { _stretchPoint = value; }
        }

        #endregion

        /// <summary>
        /// initialize a Gdal based raster layer
        /// </summary>
        /// <param name="strLayerName">Name of layer</param>
        /// <param name="imageFilename">location of image</param>
        public GdalRasterLayer(string strLayerName, string imageFilename)
        {
            LayerName = strLayerName;
            Filename = imageFilename;
            disposed = false;

            Gdal.AllRegister();

            try
            {
                _gdalDataset = Gdal.OpenShared(_Filename, Access.GA_ReadOnly);

                // have gdal read the projection
                _projectionWkt = _gdalDataset.GetProjectionRef();

                // no projection info found in the image...check for a prj
                if (_projectionWkt == "" &&
                    File.Exists(imageFilename.Substring(0, imageFilename.LastIndexOf(".")) + ".prj"))
                {
                    _projectionWkt =
                        File.ReadAllText(imageFilename.Substring(0, imageFilename.LastIndexOf(".")) + ".prj");
                }

                _imagesize = new Size(_gdalDataset.RasterXSize, _gdalDataset.RasterYSize);
                _envelope = GetExtent();
                _histoBounds = new Rectangle((int) _envelope.Left, (int) _envelope.Bottom, (int) _envelope.Width,
                                             (int) _envelope.Height);
                _lbands = _gdalDataset.RasterCount;
            }
            catch (Exception ex)
            {
                _gdalDataset = null;
                throw new Exception("Couldn't load " + imageFilename + "\n\n" + ex.Message + ex.InnerException);
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public override BoundingBox Envelope
        {
            get { return _envelope; }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void Render(Graphics g, Map map)
        {
            if (disposed)
                throw (new ApplicationException("Error: An attempt was made to render a disposed layer"));

            GetPreview(_gdalDataset, map.Size, g, map.Envelope, null, map);
            base.Render(g, map);
        }

        // get raster projection
        public ICoordinateSystem GetProjection()
        {
            CoordinateSystemFactory cFac = new CoordinateSystemFactory();

            try
            {
                if (_projectionWkt != "")
                    return cFac.CreateFromWkt(_projectionWkt);
            }
            catch
            {
            }

            return null;
        }

        // zoom to native resolution
        public double GetOneToOne(Map map)
        {
            double DsWidth = _imagesize.Width;
            double DsHeight = _imagesize.Height;
            double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;

            BoundingBox bbox = map.Envelope;
            Size size = map.Size;

            // bounds of section of image to be displayed
            left = Math.Max(bbox.Left, _envelope.Left);
            top = Math.Min(bbox.Top, _envelope.Top);
            right = Math.Min(bbox.Right, _envelope.Right);
            bottom = Math.Max(bbox.Bottom, _envelope.Bottom);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _envelope.Right - _envelope.Left;
            dblImgEnvH = _envelope.Top - _envelope.Bottom;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Right - bbox.Left;
            dblWindowGndH = bbox.Top - bbox.Bottom;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width*(dblImgEnvW/dblWindowGndW);
            dblImginMapH = size.Height*(dblImgEnvH/dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && DsWidth > DsHeight) ||
                (dblImginMapW < dblImginMapH && DsWidth < DsHeight))
                return map.Zoom*(dblImginMapW/DsWidth);
                // image was turned on its side
            else
                return map.Zoom*(dblImginMapH/DsWidth);
        }

        // zooms to nearest tiff internal resolution set
        public double GetZoomNearestRSet(Map map, bool bZoomIn)
        {
            double DsWidth = _imagesize.Width;
            double DsHeight = _imagesize.Height;
            double left, top, right, bottom;
            double dblImgEnvW, dblImgEnvH, dblWindowGndW, dblWindowGndH, dblImginMapW, dblImginMapH;
            double dblTempWidth = 0;

            BoundingBox bbox = map.Envelope;
            Size size = map.Size;

            // bounds of section of image to be displayed
            left = Math.Max(bbox.Left, _envelope.Left);
            top = Math.Min(bbox.Top, _envelope.Top);
            right = Math.Min(bbox.Right, _envelope.Right);
            bottom = Math.Max(bbox.Bottom, _envelope.Bottom);

            // height and width of envelope of transformed image in ground space
            dblImgEnvW = _envelope.Right - _envelope.Left;
            dblImgEnvH = _envelope.Top - _envelope.Bottom;

            // height and width of display window in ground space
            dblWindowGndW = bbox.Right - bbox.Left;
            dblWindowGndH = bbox.Top - bbox.Bottom;

            // height and width of transformed image in pixel space
            dblImginMapW = size.Width*(dblImgEnvW/dblWindowGndW);
            dblImginMapH = size.Height*(dblImgEnvH/dblWindowGndH);

            // image was not turned on its side
            if ((dblImginMapW > dblImginMapH && DsWidth > DsHeight) ||
                (dblImginMapW < dblImginMapH && DsWidth < DsHeight))
                dblTempWidth = dblImginMapW;
            else
                dblTempWidth = dblImginMapH;

            // zoom level is within the r sets
            if (DsWidth > dblTempWidth && (DsWidth/Math.Pow(2, 8)) < dblTempWidth)
            {
                if (bZoomIn)
                {
                    for (int i = 0; i <= 8; i++)
                    {
                        if (DsWidth/Math.Pow(2, i) > dblTempWidth)
                        {
                            if (DsWidth/Math.Pow(2, i + 1) < dblTempWidth)
                                return map.Zoom*(dblTempWidth/(DsWidth/Math.Pow(2, i)));
                        }
                    }
                }
                else
                {
                    for (int i = 8; i >= 0; i--)
                    {
                        if (DsWidth/Math.Pow(2, i) < dblTempWidth)
                        {
                            if (DsWidth/Math.Pow(2, i - 1) > dblTempWidth)
                                return map.Zoom*(dblTempWidth/(DsWidth/Math.Pow(2, i)));
                        }
                    }
                }
            }


            return map.Zoom;
        }

        /// <summary>
        /// Clones the object
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public void ResetHistoRectangle()
        {
            _histoBounds = new Rectangle((int) _envelope.Left, (int) _envelope.Bottom, (int) _envelope.Width,
                                         (int) _envelope.Height);
        }

        // gets transform between raster's native projection and the map projection
        private void GetTransform(ICoordinateSystem mapProjection)
        {
            if (mapProjection == null || _projectionWkt == "")
            {
                _transform = null;
                return;
            }

            CoordinateSystemFactory cFac = new CoordinateSystemFactory();

            // get our two projections
            ICoordinateSystem srcCoord = cFac.CreateFromWkt(_projectionWkt);
            ICoordinateSystem tgtCoord = mapProjection;

            // raster and map are in same projection, no need to transform
            if (srcCoord.WKT == tgtCoord.WKT)
            {
                _transform = null;
                return;
            }

            // create transform
            _transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(srcCoord, tgtCoord);
        }

        // get boundary of raster
        private BoundingBox GetExtent()
        {
            if (_gdalDataset != null)
            {
                double right = 0, left = 0, top = 0, bottom = 0;
                double dblW, dblH;

                double[] geoTrans = new double[6];


                _gdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!_useRotation && !_haveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                    geoTrans = new[] {999.5, 1, 0, 1000.5, 0, -1};

                _geoTransform = new GeoTransform(geoTrans);

                // image pixels
                dblW = _imagesize.Width;
                dblH = _imagesize.Height;

                left = _geoTransform.EnvelopeLeft(dblW, dblH);
                right = _geoTransform.EnvelopeRight(dblW, dblH);
                top = _geoTransform.EnvelopeTop(dblW, dblH);
                bottom = _geoTransform.EnvelopeBottom(dblW, dblH);

                return new BoundingBox(left, bottom, right, top);
            }

            return null;
        }

        // get 4 corners of image
        public Collection<Geometries.Point> GetFourCorners()
        {
            Collection<Geometries.Point> points = new Collection<Geometries.Point>();
            double[] dblPoint;

            if (_gdalDataset != null)
            {
                double[] geoTrans = new double[6];
                _gdalDataset.GetGeoTransform(geoTrans);

                // no rotation...use default transform
                if (!_useRotation && !_haveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                    geoTrans = new[] {999.5, 1, 0, 1000.5, 0, -1};

                points.Add(new Geometries.Point(geoTrans[0], geoTrans[3]));
                points.Add(new Geometries.Point(geoTrans[0] + (geoTrans[1]*_imagesize.Width),
                                                geoTrans[3] + (geoTrans[4]*_imagesize.Width)));
                points.Add(
                    new Geometries.Point(geoTrans[0] + (geoTrans[1]*_imagesize.Width) + (geoTrans[2]*_imagesize.Height),
                                         geoTrans[3] + (geoTrans[4]*_imagesize.Width) + (geoTrans[5]*_imagesize.Height)));
                points.Add(new Geometries.Point(geoTrans[0] + (geoTrans[2]*_imagesize.Height),
                                                geoTrans[3] + (geoTrans[5]*_imagesize.Height)));

                // transform to map's projection
                if (_transform != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        dblPoint = _transform.MathTransform.Transform(new[] {points[i].X, points[i].Y});
                        points[i] = new Geometries.Point(dblPoint[0], dblPoint[1]);
                    }
                }
            }

            return points;
        }

        public Polygon GetFootprint()
        {
            LinearRing myRing = new LinearRing(GetFourCorners());
            return new Polygon(myRing);
        }

        // applies map projection transfrom to get reprojected envelope
        private void ApplyTransformToEnvelope()
        {
            double[] leftBottom, leftTop, rightTop, rightBottom;
            double left, right, bottom, top;

            _envelope = GetExtent();

            if (_transform == null)
                return;

            // set envelope
            _envelope = GeometryTransform.TransformBox(_envelope, _transform.MathTransform);

            // do same to histo rectangle
            leftBottom = new double[] {_histoBounds.Left, _histoBounds.Bottom};
            leftTop = new double[] {_histoBounds.Left, _histoBounds.Top};
            rightBottom = new double[] {_histoBounds.Right, _histoBounds.Bottom};
            rightTop = new double[] {_histoBounds.Right, _histoBounds.Top};

            // transform corners into new projection
            leftBottom = _transform.MathTransform.Transform(leftBottom);
            leftTop = _transform.MathTransform.Transform(leftTop);
            rightBottom = _transform.MathTransform.Transform(rightBottom);
            rightTop = _transform.MathTransform.Transform(rightTop);

            // find extents
            left = Math.Min(leftBottom[0], Math.Min(leftTop[0], Math.Min(rightBottom[0], rightTop[0])));
            right = Math.Max(leftBottom[0], Math.Max(leftTop[0], Math.Max(rightBottom[0], rightTop[0])));
            bottom = Math.Min(leftBottom[1], Math.Min(leftTop[1], Math.Min(rightBottom[1], rightTop[1])));
            top = Math.Max(leftBottom[1], Math.Max(leftTop[1], Math.Max(rightBottom[1], rightTop[1])));

            // set histo rectangle
            _histoBounds = new Rectangle((int) left, (int) bottom, (int) right, (int) top);
        }

        // public method to set envelope and transform to new projection
        public void ReprojectToMap(Map map)
        {
            GetTransform(null);
            ApplyTransformToEnvelope();
        }

        // add image pixels to the map
        protected virtual void GetPreview(Dataset dataset, Size size, Graphics g,
                                          BoundingBox displayBbox, ICoordinateSystem mapProjection, Map map)
        {
            double[] geoTrans = new double[6];
            _gdalDataset.GetGeoTransform(geoTrans);

            // not rotated, use faster display method
            if ((!_useRotation ||
                 (geoTrans[1] == 1 && geoTrans[2] == 0 && geoTrans[4] == 0 && Math.Abs(geoTrans[5]) == 1))
                && !_haveSpot && _transform == null)
            {
                GetNonRotatedPreview(dataset, size, g, displayBbox, mapProjection);
                return;
            }
                // not rotated, but has spot...need default rotation
            else if ((geoTrans[0] == 0 && geoTrans[3] == 0) && _haveSpot)
                geoTrans = new[] {999.5, 1, 0, 1000.5, 0, -1};

            _geoTransform = new GeoTransform(geoTrans);
            double DsWidth = _imagesize.Width;
            double DsHeight = _imagesize.Height;
            double left, top, right, bottom;
            double GndX = 0, GndY = 0, ImgX = 0, ImgY = 0, PixX, PixY;
            double[] intVal = new double[Bands];
            double imageVal = 0, SpotVal = 0;
            double bitScalar = 1.0;
            Bitmap bitmap = null;
            Point bitmapTL = new Point(), bitmapBR = new Point();
            Geometries.Point imageTL = new Geometries.Point(), imageBR = new Geometries.Point();
            BoundingBox shownImageBbox, trueImageBbox;
            int bitmapLength, bitmapHeight;
            int displayImageLength, displayImageHeight;

            int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((displayBbox.Left > _envelope.Right) || (displayBbox.Right < _envelope.Left)
                    || (displayBbox.Top < _envelope.Bottom) || (displayBbox.Bottom > _envelope.Top))
                    return;

                // init histo
                _histogram = new List<int[]>();
                for (int i = 0; i < _lbands + 1; i++)
                    _histogram.Add(new int[256]);

                // bounds of section of image to be displayed
                left = Math.Max(displayBbox.Left, _envelope.Left);
                top = Math.Min(displayBbox.Top, _envelope.Top);
                right = Math.Min(displayBbox.Right, _envelope.Right);
                bottom = Math.Max(displayBbox.Bottom, _envelope.Bottom);

                trueImageBbox = new BoundingBox(left, bottom, right, top);

                // put display bounds into current projection
                if (_transform != null)
                    shownImageBbox = GeometryTransform.TransformBox(trueImageBbox, _transform.MathTransform.Inverse());
                else
                    shownImageBbox = trueImageBbox;

                // find min/max x and y pixels needed from image
                imageBR.X =
                    (int)
                    (Math.Max(_geoTransform.GroundToImage(shownImageBbox.TopLeft).X,
                              Math.Max(_geoTransform.GroundToImage(shownImageBbox.TopRight).X,
                                       Math.Max(_geoTransform.GroundToImage(shownImageBbox.BottomLeft).X,
                                                _geoTransform.GroundToImage(shownImageBbox.BottomRight).X))) + 1);
                imageBR.Y =
                    (int)
                    (Math.Max(_geoTransform.GroundToImage(shownImageBbox.TopLeft).Y,
                              Math.Max(_geoTransform.GroundToImage(shownImageBbox.TopRight).Y,
                                       Math.Max(_geoTransform.GroundToImage(shownImageBbox.BottomLeft).Y,
                                                _geoTransform.GroundToImage(shownImageBbox.BottomRight).Y))) + 1);
                imageTL.X =
                    (int)
                    Math.Min(_geoTransform.GroundToImage(shownImageBbox.TopLeft).X,
                             Math.Min(_geoTransform.GroundToImage(shownImageBbox.TopRight).X,
                                      Math.Min(_geoTransform.GroundToImage(shownImageBbox.BottomLeft).X,
                                               _geoTransform.GroundToImage(shownImageBbox.BottomRight).X)));
                imageTL.Y =
                    (int)
                    Math.Min(_geoTransform.GroundToImage(shownImageBbox.TopLeft).Y,
                             Math.Min(_geoTransform.GroundToImage(shownImageBbox.TopRight).Y,
                                      Math.Min(_geoTransform.GroundToImage(shownImageBbox.BottomLeft).Y,
                                               _geoTransform.GroundToImage(shownImageBbox.BottomRight).Y)));

                // stay within image
                if (imageBR.X > _imagesize.Width)
                    imageBR.X = _imagesize.Width;
                if (imageBR.Y > _imagesize.Height)
                    imageBR.Y = _imagesize.Height;
                if (imageTL.Y < 0)
                    imageTL.Y = 0;
                if (imageTL.X < 0)
                    imageTL.X = 0;

                displayImageLength = (int) (imageBR.X - imageTL.X);
                displayImageHeight = (int) (imageBR.Y - imageTL.Y);

                // find ground coordinates of image pixels
                Geometries.Point groundBR = _geoTransform.ImageToGround(imageBR);
                Geometries.Point groundTL = _geoTransform.ImageToGround(imageTL);

                // convert ground coordinates to map coordinates to figure out where to place the bitmap
                bitmapBR = new Point((int) map.WorldToImage(trueImageBbox.BottomRight).X + 1,
                                     (int) map.WorldToImage(trueImageBbox.BottomRight).Y + 1);
                bitmapTL = new Point((int) map.WorldToImage(trueImageBbox.TopLeft).X,
                                     (int) map.WorldToImage(trueImageBbox.TopLeft).Y);

                bitmapLength = bitmapBR.X - bitmapTL.X;
                bitmapHeight = bitmapBR.Y - bitmapTL.Y;

                // check to see if image is on its side
                if (bitmapLength > bitmapHeight && displayImageLength < displayImageHeight)
                {
                    displayImageLength = bitmapHeight;
                    displayImageHeight = bitmapLength;
                }
                else
                {
                    displayImageLength = bitmapLength;
                    displayImageHeight = bitmapHeight;
                }

                // scale
                if (_bitDepth == 12)
                    bitScalar = 16.0;
                else if (_bitDepth == 16)
                    bitScalar = 256.0;
                else if (_bitDepth == 32)
                    bitScalar = 16777216.0;

                // 0 pixels in length or height, nothing to display
                if (bitmapLength < 1 || bitmapHeight < 1)
                    return;

                //initialize bitmap
                bitmap = new Bitmap(bitmapLength, bitmapHeight, PixelFormat.Format24bppRgb);
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmapLength, bitmapHeight),
                                                        ImageLockMode.ReadWrite, bitmap.PixelFormat);

                try
                {
                    unsafe
                    {
                        // turn everything yellow, so we can make fill transparent
                        for (int y = 0; y < bitmapHeight; y++)
                        {
                            byte* brow = (byte*) bitmapData.Scan0 + (y*bitmapData.Stride);
                            for (int x = 0; x < bitmapLength; x++)
                            {
                                brow[x*3 + 0] = 0;
                                brow[x*3 + 1] = 255;
                                brow[x*3 + 2] = 255;
                            }
                        }

                        // create 3 dimensional buffer [band][x pixel][y pixel]
                        double[][] tempBuffer = new double[Bands][];
                        double[][][] buffer = new double[Bands][][];
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[displayImageLength][];
                            for (int j = 0; j < displayImageLength; j++)
                                buffer[i][j] = new double[displayImageHeight];
                        }

                        Band[] band = new Band[Bands];
                        int[] ch = new int[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            tempBuffer[i] = new double[displayImageLength*displayImageHeight];
                            band[i] = dataset.GetRasterBand(i + 1);

                            band[i].ReadRaster(
                                (int) imageTL.X,
                                (int) imageTL.Y,
                                (int) (imageBR.X - imageTL.X),
                                (int) (imageBR.Y - imageTL.Y),
                                tempBuffer[i], displayImageLength, displayImageHeight, 0, 0);

                            // parse temp buffer into the image x y value buffer
                            long pos = 0;
                            for (int y = 0; y < displayImageHeight; y++)
                            {
                                for (int x = 0; x < displayImageLength; x++, pos++)
                                    buffer[i][x][y] = tempBuffer[i][pos];
                            }

                            if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_BlueBand) ch[i] = 0;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GreenBand) ch[i] = 1;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_RedBand) ch[i] = 2;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_Undefined)
                                ch[i] = 3; // infrared
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GrayIndex) ch[i] = 0;
                            else ch[i] = -1;
                        }

                        // store these values to keep from having to make slow method calls
                        int bitmapTLX = bitmapTL.X;
                        int bitmapTLY = bitmapTL.Y;
                        double imageTop = imageTL.Y;
                        double imageLeft = imageTL.X;
                        double dblMapPixelWidth = map.PixelWidth;
                        double dblMapPixelHeight = map.PixelHeight;
                        double dblMapMinX = map.Envelope.Min.X;
                        double dblMapMaxY = map.Envelope.Max.Y;
                        double geoTop, geoLeft, geoHorzPixRes, geoVertPixRes, geoXRot, geoYRot;

                        // get inverse values
                        geoTop = _geoTransform.Inverse[3];
                        geoLeft = _geoTransform.Inverse[0];
                        geoHorzPixRes = _geoTransform.Inverse[1];
                        geoVertPixRes = _geoTransform.Inverse[5];
                        geoXRot = _geoTransform.Inverse[2];
                        geoYRot = _geoTransform.Inverse[4];

                        double dblXScale = (imageBR.X - imageTL.X)/(displayImageLength - 1);
                        double dblYScale = (imageBR.Y - imageTL.Y)/(displayImageHeight - 1);
                        double[] dblPoint;

                        // get inverse transform  
                        // NOTE: calling transform.MathTransform.Inverse() once and storing it
                        // is much faster than having to call every time it is needed
                        IMathTransform inverseTransform = null;
                        if (_transform != null)
                            inverseTransform = _transform.MathTransform.Inverse();

                        for (PixY = 0; PixY < bitmapBR.Y - bitmapTL.Y; PixY++)
                        {
                            byte* row = (byte*) bitmapData.Scan0 + ((int) Math.Round(PixY)*bitmapData.Stride);

                            for (PixX = 0; PixX < bitmapBR.X - bitmapTL.X; PixX++)
                            {
                                // same as Map.ImageToGround(), but much faster using stored values...rather than called each time
                                GndX = dblMapMinX + (PixX + bitmapTLX)*dblMapPixelWidth;
                                GndY = dblMapMaxY - (PixY + bitmapTLY)*dblMapPixelHeight;

                                // transform ground point if needed
                                if (_transform != null)
                                {
                                    dblPoint = inverseTransform.Transform(new[] {GndX, GndY});
                                    GndX = dblPoint[0];
                                    GndY = dblPoint[1];
                                }

                                // same as GeoTransform.GroundToImage(), but much faster using stored values...
                                ImgX = (geoLeft + geoHorzPixRes*GndX + geoXRot*GndY);
                                ImgY = (geoTop + geoYRot*GndX + geoVertPixRes*GndY);

                                if (ImgX < imageTL.X || ImgX > imageBR.X || ImgY < imageTL.Y || ImgY > imageBR.Y)
                                    continue;

                                // color correction
                                for (int i = 0; i < Bands; i++)
                                {
                                    intVal[i] =
                                        buffer[i][(int) ((ImgX - imageLeft)/dblXScale)][
                                            (int) ((ImgY - imageTop)/dblYScale)];

                                    imageVal = SpotVal = intVal[i] = intVal[i]/bitScalar;

                                    if (_colorCorrect)
                                    {
                                        intVal[i] = ApplyColorCorrection(imageVal, SpotVal, ch[i], GndX, GndY);

                                        // if pixel is within ground boundary, add its value to the histogram
                                        if (ch[i] != -1 && intVal[i] > 0 && (_histoBounds.Bottom >= (int) GndY) &&
                                            _histoBounds.Top <= (int) GndY &&
                                            _histoBounds.Left <= (int) GndX && _histoBounds.Right >= (int) GndX)
                                        {
                                            _histogram[ch[i]][(int) intVal[i]]++;
                                        }
                                    }

                                    if (intVal[i] > 255)
                                        intVal[i] = 255;
                                }

                                // luminosity
                                if (_lbands >= 3)
                                    _histogram[_lbands][(int) (intVal[2]*0.2126 + intVal[1]*0.7152 + intVal[0]*0.0722)]
                                        ++;

                                WritePixel(PixX, intVal, iPixelSize, ch, row);
                            }
                        }
                    }
                }

                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            bitmap.MakeTransparent(Color.Yellow);
            if (_transparentColor != Color.Empty)
                bitmap.MakeTransparent(_transparentColor);
            g.DrawImage(bitmap, new Point(bitmapTL.X, bitmapTL.Y));
        }

        // faster than rotated display
        private void GetNonRotatedPreview(Dataset dataset, Size size, Graphics g,
                                          BoundingBox bbox, ICoordinateSystem mapProjection)
        {
            double[] geoTrans = new double[6];
            dataset.GetGeoTransform(geoTrans);

            // default transform
            if (!_useRotation && !_haveSpot || (geoTrans[0] == 0 && geoTrans[3] == 0))
                geoTrans = new[] {999.5, 1, 0, 1000.5, 0, -1};
            Bitmap bitmap = null;
            _geoTransform = new GeoTransform(geoTrans);
            int DsWidth = 0;
            int DsHeight = 0;
            BitmapData bitmapData = null;
            double[] intVal = new double[Bands];
            int p_indx;
            double bitScalar = 1.0;

            double dblImginMapW = 0, dblImginMapH = 0, dblLocX = 0, dblLocY = 0;

            int iPixelSize = 3; //Format24bppRgb = byte[b,g,r] 

            if (dataset != null)
            {
                //check if image is in bounding box
                if ((bbox.Left > _envelope.Right) || (bbox.Right < _envelope.Left)
                    || (bbox.Top < _envelope.Bottom) || (bbox.Bottom > _envelope.Top))
                    return;

                DsWidth = _imagesize.Width;
                DsHeight = _imagesize.Height;

                _histogram = new List<int[]>();
                for (int i = 0; i < _lbands + 1; i++)
                    _histogram.Add(new int[256]);

                double left = Math.Max(bbox.Left, _envelope.Left);
                double top = Math.Min(bbox.Top, _envelope.Top);
                double right = Math.Min(bbox.Right, _envelope.Right);
                double bottom = Math.Max(bbox.Bottom, _envelope.Bottom);

                double x1 = Math.Abs(_geoTransform.PixelX(left));
                double y1 = Math.Abs(_geoTransform.PixelY(top));
                double imgPixWidth = _geoTransform.PixelXwidth(right - left);
                double imgPixHeight = _geoTransform.PixelYwidth(bottom - top);

                //get screen pixels image should fill 
                double dblBBoxW = bbox.Right - bbox.Left;
                double dblBBoxtoImgPixX = imgPixWidth/dblBBoxW;
                dblImginMapW = size.Width*dblBBoxtoImgPixX*_geoTransform.HorizontalPixelResolution;


                double dblBBoxH = bbox.Top - bbox.Bottom;
                double dblBBoxtoImgPixY = imgPixHeight/dblBBoxH;
                dblImginMapH = size.Height*dblBBoxtoImgPixY*-_geoTransform.VerticalPixelResolution;

                if ((dblImginMapH == 0) || (dblImginMapW == 0))
                    return;

                // ratios of bounding box to image ground space
                double dblBBoxtoImgX = size.Width/dblBBoxW;
                double dblBBoxtoImgY = size.Height/dblBBoxH;

                // set where to display bitmap in Map
                if (bbox.Left != left)
                {
                    if (bbox.Right != right)
                        dblLocX = (_envelope.Left - bbox.Left)*dblBBoxtoImgX;
                    else
                        dblLocX = size.Width - dblImginMapW;
                }
                if (bbox.Top != top)
                {
                    if (bbox.Bottom != bottom)
                        dblLocY = (bbox.Top - _envelope.Top)*dblBBoxtoImgY;
                    else
                        dblLocY = size.Height - dblImginMapH;
                }

                // scale
                if (_bitDepth == 12)
                    bitScalar = 16.0;
                else if (_bitDepth == 16)
                    bitScalar = 256.0;
                else if (_bitDepth == 32)
                    bitScalar = 16777216.0;

                try
                {
                    bitmap = new Bitmap((int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH),
                                        PixelFormat.Format24bppRgb);
                    bitmapData =
                        bitmap.LockBits(
                            new Rectangle(0, 0, (int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH)),
                            ImageLockMode.ReadWrite, bitmap.PixelFormat);

                    unsafe
                    {
                        double[][] buffer = new double[Bands][];
                        Band[] band = new Band[Bands];
                        int[] ch = new int[Bands];

                        // get data from image
                        for (int i = 0; i < Bands; i++)
                        {
                            buffer[i] = new double[(int) Math.Round(dblImginMapW)*(int) Math.Round(dblImginMapH)];
                            band[i] = dataset.GetRasterBand(i + 1);

                            band[i].ReadRaster((int) Math.Round(x1), (int) Math.Round(y1), (int) Math.Round(imgPixWidth),
                                               (int) Math.Round(imgPixHeight),
                                               buffer[i], (int) Math.Round(dblImginMapW), (int) Math.Round(dblImginMapH),
                                               0, 0);

                            if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_BlueBand) ch[i] = 0;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GreenBand) ch[i] = 1;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_RedBand) ch[i] = 2;
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_Undefined)
                                ch[i] = 3; // infrared
                            else if (band[i].GetRasterColorInterpretation() == ColorInterp.GCI_GrayIndex) ch[i] = 4;
                            else ch[i] = -1;
                        }

                        if (_bitDepth == 32)
                            ch = new[] {0, 1, 2};

                        p_indx = 0;
                        for (int y = 0; y < Math.Round(dblImginMapH); y++)
                        {
                            byte* row = (byte*) bitmapData.Scan0 + (y*bitmapData.Stride);
                            for (int x = 0; x < Math.Round(dblImginMapW); x++, p_indx++)
                            {
                                for (int i = 0; i < Bands; i++)
                                {
                                    intVal[i] = buffer[i][p_indx]/bitScalar;

                                    if (_colorCorrect)
                                    {
                                        intVal[i] = ApplyColorCorrection(intVal[i], 0, ch[i], 0, 0);

                                        if (_lbands >= 3)
                                            _histogram[_lbands][
                                                (int) (intVal[2]*0.2126 + intVal[1]*0.7152 + intVal[0]*0.0722)]++;
                                    }

                                    if (intVal[i] > 255)
                                        intVal[i] = 255;
                                }

                                WritePixel(x, intVal, iPixelSize, ch, row);
                            }
                        }
                    }
                }
                catch
                {
                    return;
                }
                finally
                {
                    if (bitmapData != null)
                        bitmap.UnlockBits(bitmapData);
                }
            }
            if (_transparentColor != Color.Empty)
                bitmap.MakeTransparent(_transparentColor);
            g.DrawImage(bitmap, new Point((int) Math.Round(dblLocX), (int) Math.Round(dblLocY)));
        }

        protected unsafe void WritePixel(double x, double[] intVal, int iPixelSize, int[] ch, byte* row)
        {
            // write out pixels
            // black and white
            if (Bands == 1 && _bitDepth != 32)
            {
                if (_showClip)
                {
                    if (intVal[0] == 0)
                    {
                        row[(int) Math.Round(x)*iPixelSize] = 255;
                        row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 2] = 0;
                    }
                    else if (intVal[0] == 255)
                    {
                        row[(int) Math.Round(x)*iPixelSize] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                        row[(int) Math.Round(x)*iPixelSize + 2] = 255;
                    }
                    else
                    {
                        row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[0];
                        row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[0];
                        row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[0];
                    }
                }
                else
                {
                    row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[0];
                    row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[0];
                    row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[0];
                }
            }
                // IR grayscale
            else if (DisplayIR && Bands == 4)
            {
                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] == 3)
                    {
                        if (_showClip)
                        {
                            if (intVal[3] == 0)
                            {
                                row[(int) Math.Round(x)*iPixelSize] = 255;
                                row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 2] = 0;
                            }
                            else if (intVal[3] == 255)
                            {
                                row[(int) Math.Round(x)*iPixelSize] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 1] = 0;
                                row[(int) Math.Round(x)*iPixelSize + 2] = 255;
                            }
                            else
                            {
                                row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[i];
                                row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[i];
                                row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[i];
                            }
                        }
                        else
                        {
                            row[(int) Math.Round(x)*iPixelSize] = (byte) intVal[i];
                            row[(int) Math.Round(x)*iPixelSize + 1] = (byte) intVal[i];
                            row[(int) Math.Round(x)*iPixelSize + 2] = (byte) intVal[i];
                        }
                    }
                    else
                        continue;
                }
            }
                // CIR
            else if (DisplayCIR && Bands == 4)
            {
                if (_showClip)
                {
                    if (intVal[0] == 0 && intVal[1] == 0 && intVal[3] == 0)
                    {
                        intVal[3] = intVal[0] = 0;
                        intVal[1] = 255;
                    }
                    else if (intVal[0] == 255 && intVal[1] == 255 && intVal[3] == 255)
                        intVal[1] = intVal[0] = 0;
                }

                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] != 0 && ch[i] != -1)
                        row[(int) Math.Round(x)*iPixelSize + ch[i] - 1] = (byte) intVal[i];
                }
            }
                // RGB
            else
            {
                if (_showClip)
                {
                    if (intVal[0] == 0 && intVal[1] == 0 && intVal[2] == 0)
                    {
                        intVal[0] = intVal[1] = 0;
                        intVal[2] = 255;
                    }
                    else if (intVal[0] == 255 && intVal[1] == 255 && intVal[2] == 255)
                        intVal[1] = intVal[2] = 0;
                }

                for (int i = 0; i < Bands; i++)
                {
                    if (ch[i] != 3 && ch[i] != -1)
                        row[(int) Math.Round(x)*iPixelSize + ch[i]] = (byte) intVal[i];
                }
            }
        }

        // apply any color correction to pixel
        private double ApplyColorCorrection(double imageVal, double spotVal, int channel, double GndX, double GndY)
        {
            double finalVal;
            double distance;
            double imagePct, spotPct;

            finalVal = imageVal;

            if (_haveSpot)
            {
                // gamma
                if (_nonSpotGamma != 1)
                    imageVal = 256*Math.Pow((imageVal/256), _nonSpotGamma);

                // gain
                if (channel == 2)
                    imageVal = imageVal*_nonSpotGain[0];
                else if (channel == 1)
                    imageVal = imageVal*_nonSpotGain[1];
                else if (channel == 0)
                    imageVal = imageVal*_nonSpotGain[2];
                else if (channel == 3)
                    imageVal = imageVal*_nonSpotGain[3];

                if (imageVal > 255)
                    imageVal = 255;

                // curve
                if (_nonSpotCurveLut != null)
                    if (_nonSpotCurveLut.Count != 0)
                    {
                        if (channel == 2 || channel == 4)
                            imageVal = _nonSpotCurveLut[0][(int) imageVal];
                        else if (channel == 1)
                            imageVal = _nonSpotCurveLut[1][(int) imageVal];
                        else if (channel == 0)
                            imageVal = _nonSpotCurveLut[2][(int) imageVal];
                        else if (channel == 3)
                            imageVal = _nonSpotCurveLut[3][(int) imageVal];
                    }

                finalVal = imageVal;

                distance = Math.Sqrt(Math.Pow(GndX - SpotPoint.X, 2) + Math.Pow(GndY - SpotPoint.Y, 2));

                if (distance <= _innerSpotRadius + _outerSpotRadius)
                {
                    // gamma
                    if (_spotGamma != 1)
                        spotVal = 256*Math.Pow((spotVal/256), _spotGamma);

                    // gain
                    if (channel == 2)
                        spotVal = spotVal*_spotGain[0];
                    else if (channel == 1)
                        spotVal = spotVal*_spotGain[1];
                    else if (channel == 0)
                        spotVal = spotVal*_spotGain[2];
                    else if (channel == 3)
                        spotVal = spotVal*_spotGain[3];

                    if (spotVal > 255)
                        spotVal = 255;

                    // curve
                    if (_spotCurveLut != null)
                        if (_spotCurveLut.Count != 0)
                        {
                            if (channel == 2 || channel == 4)
                                spotVal = _spotCurveLut[0][(int) spotVal];
                            else if (channel == 1)
                                spotVal = _spotCurveLut[1][(int) spotVal];
                            else if (channel == 0)
                                spotVal = _spotCurveLut[2][(int) spotVal];
                            else if (channel == 3)
                                spotVal = _spotCurveLut[3][(int) spotVal];
                        }

                    if (distance < _innerSpotRadius)
                        finalVal = spotVal;
                    else
                    {
                        imagePct = (distance - _innerSpotRadius)/_outerSpotRadius;
                        spotPct = 1 - imagePct;

                        finalVal = (Math.Round((spotVal*spotPct) + (imageVal*imagePct)));
                    }
                }
            }

            // gamma
            if (_gamma != 1)
                finalVal = (256*Math.Pow((finalVal/256), _gamma));


            switch (channel)
            {
                case 2:
                    finalVal = finalVal*_gain[0];
                    break;
                case 1:
                    finalVal = finalVal*_gain[1];
                    break;
                case 0:
                    finalVal = finalVal*_gain[2];
                    break;
                case 3:
                    finalVal = finalVal*_gain[3];
                    break;
            }

            if (finalVal > 255)
                finalVal = 255;

            // curve
            if (_curveLut != null)
                if (_curveLut.Count != 0)
                {
                    if (channel == 2 || channel == 4)
                        finalVal = _curveLut[0][(int) finalVal];
                    else if (channel == 1)
                        finalVal = _curveLut[1][(int) finalVal];
                    else if (channel == 0)
                        finalVal = _curveLut[2][(int) finalVal];
                    else if (channel == 3)
                        finalVal = _curveLut[3][(int) finalVal];
                }

            return finalVal;
        }

        /// <summary>
        /// Build histogram and statistics
        /// </summary>
        /// <param name="bQuick">If true, build histogram off of smaller subsample of image</param>
        public void BuildHisto(bool bQuick)
        {
            Dataset dataset = _gdalDataset;
            int height, width, Bands;
            int p_indx = 0;
            int intVal;
            double[] stdDev = new double[4];
            int maxVal;

            if (bQuick)
            {
                height = 20;
                width = (int) (20*(dataset.RasterXSize/(double) dataset.RasterYSize));
            }
            else
            {
                height = 3000; // dataset.RasterYSize;
                width = (int) (3000*(dataset.RasterXSize/(double) dataset.RasterYSize));
                // dataset.RasterXSize;
            }

            Bands = dataset.RasterCount;

            _histogram = new List<int[]>();
            _histoMean = new double[Bands];

            for (int band = 1; band <= Bands; band++)
            {
                List<object> lstObj = new List<object>();

                if (_bitDepth == 8)
                    _histogram.Add(new int[256]);
                else if (_bitDepth == 12)
                    _histogram.Add(new int[4096]);
                else
                    _histogram.Add(new int[65536]);

                for (int i = 0; i < _histogram[band - 1].Length; i++)
                    _histogram[band - 1][i] = 0;

                maxVal = _histogram[0].Length - 1;

                Band RBand = dataset.GetRasterBand(band);
                double[] buffer = new double[width*height];
                RBand.ReadRaster(0, 0, dataset.RasterXSize, dataset.RasterYSize, buffer, width, height, 0, 0);

                p_indx = 0;

                _histoMean[band - 1] = 0;

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++, p_indx++)
                    {
                        intVal = (int) buffer[p_indx];

                        // gamma
                        if (_nonSpotGamma != 1)
                            intVal = (int) (256*Math.Pow(((double) intVal/256), _nonSpotGamma));

                        // gain
                        intVal = (int) (intVal*_gain[band - 1]);

                        if (intVal > maxVal)
                            intVal = maxVal;

                        // curves
                        if (_nonSpotCurveLut != null)
                            if (_nonSpotCurveLut.Count != 0)
                                intVal = _nonSpotCurveLut[band - 1][intVal];

                        buffer[p_indx] = (byte) intVal;

                        _histogram[band - 1][intVal]++;
                        _histoMean[band - 1] += intVal;
                    }
                }
                _histoMean[band - 1] /= buffer.Length;
                stdDev[band - 1] = CalcStandardDeviation(buffer, _histoMean[band - 1]);
            }

            // set brightness and contrast
            if (Bands > 1)
            {
                _histoBrightness = (_histoMean[0]*0.2126 + _histoMean[1]*0.7152 + _histoMean[2]*0.0722)/2.55;
                _histoContrast = (stdDev[0]*0.2126 + stdDev[1]*0.7152 + stdDev[2]*0.0722)/1.28;
            }
            else
            {
                _histoBrightness = _histoMean[0]/2.55;
                _histoContrast = stdDev[0]/1.28;
            }
        }

        private double CalcStandardDeviation(double[] buffer, double mean)
        {
            double dblAccum = 0;

            for (int i = 0; i < buffer.Length; i++)
                dblAccum += Math.Pow((buffer[i] - mean), 2);

            dblAccum = dblAccum/buffer.Length;

            return Math.Sqrt(dblAccum);
        }

        // find min and max pixel values of the image
        private void ComputeStretch()
        {
            double min = 99999999, max = -99999999;
            int width, height;

            if (_gdalDataset.RasterYSize < 4000)
            {
                height = _gdalDataset.RasterYSize;
                width = _gdalDataset.RasterXSize;
            }
            else
            {
                height = 4000;
                width = (int) (4000*(_gdalDataset.RasterXSize/(double) _gdalDataset.RasterYSize));
            }

            double[] buffer = new double[width*height];

            for (int band = 1; band <= _lbands; band++)
            {
                Band RBand = _gdalDataset.GetRasterBand(band);
                RBand.ReadRaster(0, 0, _gdalDataset.RasterXSize, _gdalDataset.RasterYSize, buffer, width, height, 0, 0);

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] < min)
                        min = buffer[i];
                    if (buffer[i] > max)
                        max = buffer[i];
                }
            }

            if (_bitDepth == 12)
            {
                min /= 16;
                max /= 16;
            }
            else if (_bitDepth == 16)
            {
                min /= 256;
                max /= 256;
            }

            if (max > 255)
                max = 255;

            _stretchPoint = new Point((int) min, (int) max);
        }

        #region Disposers and finalizers

        private bool disposed;

        /// <summary>
        /// Disposes the GdalRasterLayer and release the raster file
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    if (_gdalDataset != null)
                    {
                        try
                        {
                            _gdalDataset.Dispose();
                        }
                        finally
                        {
                            _gdalDataset = null;
                        }
                    }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~GdalRasterLayer()
        {
            Dispose(true);
        }

        #endregion
    }
}