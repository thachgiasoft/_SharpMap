﻿using System;
using System.Collections.Generic;
using System.Text;
using SharpMap.Layers;

namespace SharpMap.Renderer
{
    public class LayerRenderedEventArgs
        : EventArgs
    {
        private readonly ILayer _lyr;
        ILayer Layer { get { return _lyr; } }
        public LayerRenderedEventArgs(ILayer lyr)
            : base()
        {
            _lyr = lyr;
        }

    }
}