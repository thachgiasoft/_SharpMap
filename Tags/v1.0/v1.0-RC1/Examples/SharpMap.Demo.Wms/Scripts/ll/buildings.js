(function(u){function ja(a){i=u.document.createElement("canvas");i.style.webkitTransform="translate3d(0,0,0)";i.style.position="absolute";i.style.pointerEvents="none";i.style.left=0;i.style.top=0;i.style.zIndex=1E3;a.appendChild(i);k=i.getContext("2d");k.lineCap="round";k.lineJoin="round";k.lineWidth=1;try{k.mozImageSmoothingEnabled=!1}catch(c){}}function v(a,c){var b={},a=a/N,c=c/N;b[y]=0>=c?90:1<=c?-90:ka*(2*la(ma(t*(1-2*c)))-aa);b[q]=360*(1===a?1:(a%1+1)%1)-180;return b}function na(a,c){return a.replace(/\{ *([\w_]+) *\}/g,
function(a,d){return c[d]||""})}function oa(a,c){var b=new XMLHttpRequest;b.onreadystatechange=function(){4!==b.readyState||!b.status||200>b.status||299<b.status||b.responseText&&c(JSON.parse(b.responseText))};b.open("GET",a);b.send(null);return b}function H(){if(O&&!(o<I)){var a=v(A-z,B-w),c=v(A+z,B+w);J&&J.abort();J=oa(na(O,{w:a[q],n:a[y],e:c[q],s:c[y],z:o}),pa)}}function ba(a,c,b){var b=b||[],d=a[0]?a:a.features,f,g,e;if(d){a=0;for(f=d.length;a<f;a++)ba(d[a],c,b);return b}"Feature"===a.type&&(f=
a.geometry,g=a.properties);if("Polygon"==f.type&&g.height){d=f.coordinates[0];e=[];a=0;for(f=d.length;a<f;a++)c?(e.push(d[a][1]),e.push(d[a][0])):(e.push(d[a][0]),e.push(d[a][1]));b.push([g.height,qa(e)])}return b}function ca(a,c,b){for(var d,f,g=[],e,j,h,k=P-c,m=0,l=a.length;m<l;m++){e=a[m][0];j=a[m][1];h=new Int32Array(j.length);for(var i=0,r=j.length-1;i<r;i+=2){d=j[i+1];f=da<<c;var o=Q(1,ra(0,0.5-sa(ta(ua+aa*j[i]/180))/t/2));d=~~((d/360+0.5)*f);f=~~(o*f);h[i]=d;h[i+1]=f}g[m]=[Q(e>>k,ea),h,b]}return g}
function qa(a){for(var c=a.length,b=maxS=a[0],d=maxW=a[1],f,g,e=0;e<c-1;e+=2)if(a[e+1]<maxW?(maxW=a[e+1],f=e):a[e+1]>d&&(d=a[e+1],g=e),a[e]>b)b=a[e],NI=e;b=f-NI;g-=NI;0>b&&(b+=c);0>g&&(g+=c);if("CW"===(b>g?"CW":"CCW"))return a;c=[];for(g=a.length-2;0<=g;g-=2)c.push(a[g]),c.push(a[g+1]);return c}function fa(a,c){z=a;w=c;C=~~(z/2);K=~~(w/2);E=C;F=w;i.width=z;i.height=w}function ga(a,c){A=a;B=c}function ha(a){o=a;N=da<<o;L=1-0.3*(o-I)/(P-I)}function va(){R=!0;p()}function pa(a){var c,b,d,f=[],g=0,e=
0;J=null;if(a&&a.meta.z===o){d=a.meta;b=a.data;if(l&&j&&l.z===d.z){g=l.x-d.x;e=l.y-d.y;a=0;for(c=j.length;a<c;a++)f[a]=j[a][G][0]+g+","+(j[a][G][1]+e)}l=d;j=[];a=0;for(c=b.length;a<c;a++)j[a]=b[a],j[a][M]=Q(j[a][M],ea),d=j[a][G][0]+","+j[a][G][1],j[a][S]=!(f&&~f.indexOf(d));ia()}}function ia(){D=0;clearInterval(T);T=setInterval(function(){D+=0.1;if(1<D){clearInterval(T);D=1;for(var a=0,c=j.length;a<c;a++)j[a][S]=0}p()},33)}function p(){var a,c,b,d;k.clearRect(0,0,z,w);if(l&&j&&!(o<I||R)){var f=U(V,
L),g=U(W,L),e=U(X,L);k.strokeStyle=e;var i,h,p,m,x=A-C-l.x,y=B-K-l.y,r,t,n,q,s,u,v,e=0;for(i=j.length;e<i;e++){a=j[e];m=!1;c=a[G];r=new Int32Array(c.length);h=0;for(p=c.length-1;h<p;h+=2)r[h]=b=c[h]-x,r[h+1]=d=c[h+1]-y,m||(m=0<b&&b<z&&0<d&&d<w);if(m){k.fillStyle=f;h=a[S]?a[M]*D:a[M];m=Y/(Y-h);t=new Int32Array(r.length-2);n=[];h=0;for(p=r.length-1-2;h<p;h+=2)q=r[h],s=r[h+1],u=r[h+2],v=r[h+3],b=~~((q-E)*m+E),d=~~((s-F)*m+F),a=~~((u-E)*m+E),c=~~((v-F)*m+F),(u-q)*(d-s)>(b-q)*(v-s)?(n.length||(n.unshift(s),
n.unshift(q),n.push(b),n.push(d)),n.unshift(v),n.unshift(u),n.push(a),n.push(c)):(Z(n),n=[]),t[h]=b,t[h+1]=d;Z(n);k.fillStyle=g;Z(t,$)}}}}function Z(a,c){if(a.length){k.beginPath();k.moveTo(a[0],a[1]);for(var b=2,d=a.length;b<d;b+=2)k.lineTo(a[b],a[b+1]);k.closePath();c&&k.stroke();k.fill()}}function U(a,c){var b=a.match(/rgba?\((\d+),(\d+),(\d+)(,([\d.]+))?\)/);return"rgba("+[b[1],b[2],b[3],b[4]?c*b[5]:c].join()+")"}u.Int32Array=u.Int32Array||Array;var ma=Math.exp,sa=Math.log,ta=Math.tan,la=Math.atan,
Q=Math.min,ra=Math.max,t=Math.PI,aa=t/2,ua=t/4,ka=180/t,y="latitude",q="longitude",M=0,G=1,S=2,z=0,w=0,C=0,K=0,A=0,B=0,o,N,J,i,k,O,$,V="rgb(200,190,180)",W="rgb(250,240,230)",X="rgb(145,140,135)",s,l,j,L=1,D=1,T,da=256,I=14,P,E=C,F=w,Y=400,ea=Y-50,R=!1,x=u.OSMBuildings=function(a){this.addTo(a)};x.prototype.VERSION="0.1a";x.prototype.render=function(){if(this.map)return p(),this};x.prototype.setStyle=function(a){if(this.map)return a=a||{},$=void 0!==a.strokeRoofs?a.strokeRoofs:$,V=a.wallColor||V,
W=a.roofColor||W,X=a.strokeColor||X,this};x.prototype.setData=function(a,c){if(this.map)return a?(s=ba(a,c),l={n:90,w:-180,s:-90,e:180,x:0,y:0,z:o},j=ca(s,o,!0),ia()):(s=null,p()),this};x.prototype.loadData=function(a){if(this.map)return O=a,H(),this};(function(a){a.VERSION+="-leaflet-patch";a.addTo=function(a){function b(){var b=a._size.divideBy(2);return a._getTopLeftPoint().add(b)}this.map=a;ja(document.querySelector(".leaflet-container"));P=a._layersMaxZoom;fa(a._size.x,a._size.y);var d=b();ga(d.x,
d.y);ha(a._zoom);var f;window.addEventListener("resize",function(){f=setTimeout(function(){clearTimeout(f);fa(a._size.x,a._size.y);p();H()},100)},!1);a.on({move:function(){var a=b();ga(a.x,a.y);p()},moveend:function(){b();var a=v(A-C,B-K),c=v(A+C,B+K);l&&(a[y]>l.n||a[q]<l.w||c[y]<l.s||c[q]>l.e)&&H()},zoomstart:va,zoomend:function(){var b=a._zoom;R=!1;ha(b);s?(j=ca(s,o),p()):H()}});a.attributionControl.addAttribution('Buildings &copy; <a href="http://osmbuildings.org">OSM Buildings</a>');return this};
a.removeFrom=function(a){a.attributionControl.removeAttribution('Buildings &copy; <a href="http://osmbuildings.org">OSM Buildings</a>');a.off({});i.parentNode.removeChild(i);this.map=null;return this}})(x.prototype)})(this);
