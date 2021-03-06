﻿// Copyright 2011 - Felix Obermaier (www.ivv-aachen.de)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SharpMap.Styles.Symbolizer
{
    /// <summary>
    /// Text Point Style
    /// </summary>
    [Serializable]
    public class CharacterPointSymbolizer : PointSymbolizer
    {
        private string _text;

        private int _characterIndex;
        
        /// <summary>
        /// Initializes a new TextPointStyle
        /// </summary>
        public CharacterPointSymbolizer()
        {
            Font = new Font("Wingdings", 12);
            CharacterIndex = 0xa5;
            Foreground = Brushes.Firebrick;
            Halo = 0;
            HaloBrush = Brushes.Transparent;
            StringFormat = new StringFormat(StringFormatFlags.NoClip){ Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.None };
        }
        
        /// <summary>
        /// Gets or sets the font to symbolize
        /// </summary>
        /// <remarks>The default value is <c>Wingdings</c></remarks>
        public Font Font { get; set; }

        /// <summary>
        /// Gets or sets the brush to symbolize
        /// </summary>
        /// <remarks>
        /// The default value is <c>Brushes.Firebrick</c>
        /// </remarks>
        public Brush Foreground { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Halo { get; set; }

        /// <summary>
        /// Gets or sets the brush to render the halo
        /// </summary>
        /// <remarks>
        /// The default value is <c>Brushes.Transparent</c>
        /// </remarks>
        public Brush HaloBrush { get; set; }

        /// <summary>
        /// The index of the character, -1 if the <see cref="Text"/> is longer than 1 character.
        /// </summary>
        public int CharacterIndex 
        {
            get {return _characterIndex;}
            set
            {
                _characterIndex = value;
                _text = Convert.ToString(Convert.ToChar(_characterIndex));
            }
        }

        /// <summary>
        /// The text for the symbol
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                
                _text = value;

                if (_text.Length > 1)
                    _characterIndex = -1;
                else
                {
                    _characterIndex = Convert.ToInt32(Convert.ToChar(_text[0]));
                }
            }
        }

        /// <summary>
        /// The format for symbolization
        /// </summary>
        public StringFormat StringFormat { get; set; }

        public override Size Size
        {
            get
            {
                return Size.Empty;
                /*
                var bmp = new Bitmap(1,1);
                using (var g = Graphics.FromImage(bmp))
                {
                    var sizef = Rendering.VectorRenderer.SizeOfString(g, _text, Font);
                    //g.MeasureString(_text, Font, 1024, StringFormat);
                    return sizef.ToSize();
                }
                 */

            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        public override float  Scale
        {
            get
            {
                return 1;
            }
            set { }
        }

        internal override void OnRenderInternal(PointF pt, Graphics g)
        {
            if (Halo > 0)
            {
                //need to look it up
                GraphicsPath path = new GraphicsPath(FillMode.Winding);
                path.AddString(_text, Font.FontFamily, (int)Font.Style, Font.Size, pt, StringFormat);
                g.DrawPath(new Pen(HaloBrush, 2*Halo), path);
                g.FillPath(Foreground, path);
            }
            else
            {
                g.DrawString(_text, Font, Foreground, pt, StringFormat);
            }
        }
    }
}