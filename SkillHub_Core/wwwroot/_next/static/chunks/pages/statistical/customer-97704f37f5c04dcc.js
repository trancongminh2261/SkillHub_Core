(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[5068],{80268:function(e,t,a){(window.__NEXT_P=window.__NEXT_P||[]).push(["/statistical/customer",function(){return a(68546)}])},28864:function(e,t,a){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),function(e,t){for(var a in t)Object.defineProperty(e,a,{enumerable:!0,get:t[a]})}(t,{default:function(){return s},noSSR:function(){return i}});let l=a(38754);a(85893),a(67294);let n=l._(a(56016));function r(e){return{default:(null==e?void 0:e.default)||e}}function i(e,t){return delete t.webpack,delete t.modules,e(t)}function s(e,t){let a=n.default,l={loading:e=>{let{error:t,isLoading:a,pastDelay:l}=e;return null}};e instanceof Promise?l.loader=()=>e:"function"==typeof e?l.loader=e:"object"==typeof e&&(l={...l,...e});let s=(l={...l,...t}).loader;return(l.loadableGenerated&&(l={...l,...l.loadableGenerated},delete l.loadableGenerated),"boolean"!=typeof l.ssr||l.ssr)?a({...l,loader:()=>null!=s?s().then(r):Promise.resolve(r(()=>null))}):(delete l.webpack,delete l.modules,i(a,l))}("function"==typeof t.default||"object"==typeof t.default&&null!==t.default)&&void 0===t.default.__esModule&&(Object.defineProperty(t.default,"__esModule",{value:!0}),Object.assign(t.default,t),e.exports=t.default)},60572:function(e,t,a){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),Object.defineProperty(t,"LoadableContext",{enumerable:!0,get:function(){return l}});let l=a(38754)._(a(67294)).default.createContext(null)},56016:function(e,t,a){"use strict";/**
@copyright (c) 2017-present James Kyle <me@thejameskyle.com>
 MIT License
 Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:
 The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE
*/Object.defineProperty(t,"__esModule",{value:!0}),Object.defineProperty(t,"default",{enumerable:!0,get:function(){return h}});let l=a(38754)._(a(67294)),n=a(60572),r=[],i=[],s=!1;function o(e){let t=e(),a={loading:!0,loaded:null,error:null};return a.promise=t.then(e=>(a.loading=!1,a.loaded=e,e)).catch(e=>{throw a.loading=!1,a.error=e,e}),a}class d{promise(){return this._res.promise}retry(){this._clearTimeouts(),this._res=this._loadFn(this._opts.loader),this._state={pastDelay:!1,timedOut:!1};let{_res:e,_opts:t}=this;e.loading&&("number"==typeof t.delay&&(0===t.delay?this._state.pastDelay=!0:this._delay=setTimeout(()=>{this._update({pastDelay:!0})},t.delay)),"number"==typeof t.timeout&&(this._timeout=setTimeout(()=>{this._update({timedOut:!0})},t.timeout))),this._res.promise.then(()=>{this._update({}),this._clearTimeouts()}).catch(e=>{this._update({}),this._clearTimeouts()}),this._update({})}_update(e){this._state={...this._state,error:this._res.error,loaded:this._res.loaded,loading:this._res.loading,...e},this._callbacks.forEach(e=>e())}_clearTimeouts(){clearTimeout(this._delay),clearTimeout(this._timeout)}getCurrentValue(){return this._state}subscribe(e){return this._callbacks.add(e),()=>{this._callbacks.delete(e)}}constructor(e,t){this._loadFn=e,this._opts=t,this._callbacks=new Set,this._delay=null,this._timeout=null,this.retry()}}function u(e){return function(e,t){let a=Object.assign({loader:null,loading:null,delay:200,timeout:null,webpack:null,modules:null},t),r=null;function o(){if(!r){let t=new d(e,a);r={getCurrentValue:t.getCurrentValue.bind(t),subscribe:t.subscribe.bind(t),retry:t.retry.bind(t),promise:t.promise.bind(t)}}return r.promise()}if(!s){let e=a.webpack?a.webpack():a.modules;e&&i.push(t=>{for(let a of e)if(t.includes(a))return o()})}function u(e,t){!function(){o();let e=l.default.useContext(n.LoadableContext);e&&Array.isArray(a.modules)&&a.modules.forEach(t=>{e(t)})}();let i=l.default.useSyncExternalStore(r.subscribe,r.getCurrentValue,r.getCurrentValue);return l.default.useImperativeHandle(t,()=>({retry:r.retry}),[]),l.default.useMemo(()=>{var t;return i.loading||i.error?l.default.createElement(a.loading,{isLoading:i.loading,pastDelay:i.pastDelay,timedOut:i.timedOut,error:i.error,retry:r.retry}):i.loaded?l.default.createElement((t=i.loaded)&&t.default?t.default:t,e):null},[e,i])}return u.preload=()=>o(),u.displayName="LoadableComponent",l.default.forwardRef(u)}(o,e)}function c(e,t){let a=[];for(;e.length;){let l=e.pop();a.push(l(t))}return Promise.all(a).then(()=>{if(e.length)return c(e,t)})}u.preloadAll=()=>new Promise((e,t)=>{c(r).then(e,t)}),u.preloadReady=e=>(void 0===e&&(e=[]),new Promise(t=>{let a=()=>(s=!0,t());c(i,e).then(a,a)})),window.__NEXT_PRELOADREADY=u.preloadReady;let h=u},68973:function(e,t,a){"use strict";var l=a(85893),n=a(67294),r=a(5152),i=a.n(r),s=a(66946),o=a(72833);let d=i()(()=>Promise.all([a.e(2774),a.e(1030)]).then(a.bind(a,92774)).then(e=>{let{Line:t}=e;return t}),{loadableGenerated:{webpack:()=>[92774]},ssr:!1});t.Z=(0,n.memo)(e=>{let{dataList:t,loading:a,colorFunction:n,xField:r,yField:i,smooth:u,className:c,haveSlider:h,legendPos:m,...p}=e;console.log("dataList",t);let f={data:t,xField:r,yField:i,xAxis:{label:{formatter:e=>"".concat((0,o.VW)(e))}},yAxis:{label:{formatter:e=>"".concat((0,o.VW)(e))}},legend:{position:m||"top"},tooltip:{formatter:e=>({name:e.type,value:"".concat(Intl.NumberFormat().format(e[i]))})},slider:null,smooth:u,animation:{appear:{animation:"path-in",duration:2e3}}};h&&(f.slider={start:0,end:1});let x={...f,...p,loading:a,className:"".concat(a?"hidden":"")};return(0,l.jsxs)("div",{className:"".concat(c||"h-[350px] relative"),children:[(0,l.jsx)(d,{...x}),!a&&(null==t?void 0:t.length)==0&&(0,l.jsx)("div",{className:"absolute left-[50%] top-[50%] translate-y-[-50%] translate-x-[-50%]",children:(0,l.jsx)(s.Z,{description:(0,l.jsx)("span",{className:"text-gray-300",children:"Kh\xf4ng c\xf3 dữ liệu"})})})]})})},44011:function(e,t,a){"use strict";var l=a(85893),n=a(67294),r=a(5152),i=a.n(r),s=a(66946),o=a(72833),d=a(58416);let u=i()(()=>Promise.all([a.e(2774),a.e(1030)]).then(a.bind(a,92774)).then(e=>{let{Pie:t}=e;return t}),{loadableGenerated:{webpack:()=>[92774]},ssr:!1}),c=["#1abc9c","#2ecc71","#3498db","#9b59b6","#34495e","#f1c40f","#e67e22","#e74c3c","#95a5a6"];t.Z=(0,n.memo)(e=>{var t;let{loading:a,colorFunction:n,legendPosition:r,isDonut:i,isPercent:h,className:m,...p}=e;function f(e){if(p.data)for(let a=0;a<p.data.length;a++){if(!(0,o.t)(p.data[a],["detail"]))return null;if(p.data[a][p.colorField]==e){var t;return null===(t=p.data[a])||void 0===t?void 0:t.detail}}}let x={appendPadding:10,data:p.data,radius:.8,innerRadius:i?.6:0,color:n||c,legend:{position:r||"right"},label:{type:"spider",content:e=>0===e[p.colorField]?"":h?"".concat(Math.round(1e3*e.percent)/10," %"):(0,o.VW)(e.value)},interactions:[{type:"pie-legend-active"},{type:"element-active"}],tooltip:{formatter:e=>({name:e[p.colorField],value:"".concat(f(e[p.colorField])?d.Jy.numberToPrice(f(e[p.colorField])):d.Jy.numberToPrice(e[p.angleField]))})},...p,loading:a,className:"".concat(a?"hidden":""),statistic:{title:{offsetY:-4,customHtml:(e,t,a)=>{var l;return'<p style="font-size: 14px; line-height: 1.4">'.concat((null===(l=p.data)||void 0===l?void 0:l.length)>0?"Tổng":"","<p>")}},content:{style:{whiteSpace:"pre-wrap",overflow:"hidden",textOverflow:"ellipsis",fontSize:"20px"},customHtml:(e,t,a,l)=>"<p>".concat((0,o.Gv)(l)?"":d.Jy.numberToPrice(l.reduce((e,t)=>e+t[p.angleField],0)),"</p>")}}};return(0,l.jsxs)("div",{className:"".concat(m||"h-[280px] relative"),children:[(0,l.jsx)(u,{...x}),(null===(t=p.data)||void 0===t?void 0:t.length)==0&&(0,l.jsx)("div",{className:"absolute left-[50%] top-[50%] translate-y-[-50%] translate-x-[-50%]",children:(0,l.jsx)(s.Z,{description:(0,l.jsx)("span",{className:"text-gray-300",children:"Kh\xf4ng c\xf3 dữ liệu"})})})]})})},35254:function(e,t,a){"use strict";var l=a(85893),n=a(36070),r=a(41664),i=a.n(r);a(67294);var s=a(65890);t.Z=e=>{let{content:t,link:a,noDetails:r}=e;return(0,l.jsx)(n.Z,{content:(0,l.jsxs)("div",{className:"w-[250px]",children:[t,". ",!r&&(0,l.jsx)("span",{children:"Chi tiết"})," ",!r&&(0,l.jsx)(i(),{href:a,children:(0,l.jsx)("a",{children:"xem tại đ\xe2y"})})]}),children:(0,l.jsx)(s.O$D,{className:"text-[#f39c12]"})})}},36693:function(e,t,a){"use strict";var l=a(67294);t.Z=()=>{let[e,t]=(0,l.useState)({width:0,height:0}),[a,n]=(0,l.useState)(!1);(0,l.useEffect)(()=>{if(0===e.height){var a,l;t({width:null===(a=window)||void 0===a?void 0:a.innerWidth,height:null===(l=window)||void 0===l?void 0:l.innerHeight})}let r=()=>{n(!0),t({width:window.innerWidth,height:window.innerHeight})};return window.addEventListener("resize",r),()=>{window.removeEventListener("resize",r)}},[]),(0,l.useEffect)(()=>{let e=()=>{n(!1)};return window.addEventListener("resize",e),()=>{window.removeEventListener("resize",e)}},[]);let r=e.width<360,i=e.width>=360&&e.width<640,s=e.width>=640&&e.width<1024,o=e.width>=1024;return{width:e.width,height:e.height,isResizing:a,isMobile:r||i,isSmartphone:r,isTablet:i,isLaptop:s,isDesktop:o}}},66734:function(e,t,a){"use strict";a.d(t,{T:function(){return n},l:function(){return l}});let l=e=>null==e?void 0:e.map(e=>({type:null==e?void 0:e.Type,value:e.Value,detail:null==e?void 0:e.ValueDetail})),n=e=>null==e?void 0:e.map(e=>({month:null==e?void 0:e.Month,type:null==e?void 0:e.Type,value:e.Value}))},55330:function(e,t,a){"use strict";var l=a(85893),n=a(96361);a(67294),t.Z=e=>{let{title:t,subTitle:a,children:r,...i}=e;return(0,l.jsx)(n.Z,{className:"shadow-sm h-full",title:(0,l.jsxs)("div",{className:"text-wrap",children:[(0,l.jsx)("p",{className:"font-[600] text-[18px]",children:t}),(0,l.jsx)("p",{className:"text-[#616161] font-[400] text-[14px]",children:a})]}),...i,children:r})}},75499:function(e,t,a){"use strict";var l=a(85893),n=a(96361),r=a(34223);a(67294),t.Z=()=>(0,l.jsx)(n.Z,{className:"shadow-sm",children:(0,l.jsxs)("div",{className:"p-[8px]",children:[(0,l.jsx)(r.Z,{active:!0,paragraph:!1,round:!0,className:"w-[70%]"}),(0,l.jsx)(r.Z,{active:!0,paragraph:!1,round:!0,style:{marginTop:30},className:"w-[60%]"}),(0,l.jsx)(r.Z,{active:!0,paragraph:!1,round:!0,style:{marginTop:14},className:"w-[40%]"})]})})},86563:function(e,t,a){"use strict";var l=a(85893),n=a(96361),r=a(77024);a(67294);var i=a(58416),s=a(41622),o=a.n(s),d=a(35254);t.Z=e=>{let{diffValue:t,status:a,title:s,value:u,lastMonth:c,detailDifference:h,infoContent:m}=e;return(0,l.jsxs)(n.Z,{className:"shadow-sm font-inter",children:[(0,l.jsxs)("div",{className:"flex items-center gap-2 mb-2",children:[(0,l.jsx)("p",{className:"font-[600] text-[18px]",children:s}),m&&(0,l.jsx)(d.Z,{content:m,noDetails:!0})]}),(0,l.jsx)("p",{className:"font-[600] text-[30px] tracking-tighter",children:i.Jy.numberToPrice(u||0)}),(0,l.jsxs)("div",{children:[1==a&&(0,l.jsxs)("div",{className:"grid grid-cols-4 gap-1 font-medium",children:[(0,l.jsxs)("div",{className:"col-span-3 flex flex-wrap items-center gap-1",children:[(0,l.jsx)("div",{className:"".concat(o().arrowUp)}),(0,l.jsx)(r.Z,{title:"Tăng ".concat(i.Jy.numberToPrice(null==h?void 0:h.toFixed(2))," so với th\xe1ng ").concat(c),children:(0,l.jsxs)("p",{className:"text-[#027A48]",children:[t,"%"]})}),(0,l.jsxs)("p",{className:"text-[#475467] ml-1",children:["so với th\xe1ng ",c]})]}),(0,l.jsx)("div",{className:"col-span-1 flex justify-end w-full",children:(0,l.jsx)("div",{className:"".concat(o().chartUp," fadeInUp")})})]}),2==a&&(0,l.jsxs)("div",{className:"grid grid-cols-4 gap-1 font-medium",children:[(0,l.jsxs)("div",{className:"col-span-3 flex flex-wrap items-center gap-1",children:[(0,l.jsx)("div",{className:"".concat(o().arrowDown)}),(0,l.jsx)(r.Z,{title:"Giảm ".concat(i.Jy.numberToPrice(null==h?void 0:h.toFixed(2))," so với th\xe1ng ").concat(c),children:(0,l.jsxs)("p",{className:"text-[#E51F1F]",children:[t,"%"]})}),(0,l.jsxs)("p",{className:"text-[#475467] ml-1",children:["so với th\xe1ng ",c]})]}),(0,l.jsx)("div",{className:"col-span-1 flex justify-end w-full",children:(0,l.jsx)("div",{className:"".concat(o().chartDown," fadeInUp")})})]}),3==a&&(0,l.jsxs)("div",{className:"grid grid-cols-4 gap-1 font-medium",children:[(0,l.jsx)("div",{className:"col-span-3 flex flex-wrap items-center gap-1",children:(0,l.jsxs)("p",{className:"text-[#475467] ml-1",children:["Kh\xf4ng đổi so với th\xe1ng ",c]})}),(0,l.jsx)("div",{className:"col-span-1 flex justify-end w-full"})]})]})]})}},68546:function(e,t,a){"use strict";a.r(t),a.d(t,{default:function(){return L}});var l=a(85893),n=a(9008),r=a.n(n),i=a(67294),s=a(53265),o=a(36054),d=a(96369),u=a(30381),c=a.n(u),h=a(86563),m=a(9473),p=a(78551),f=a(59178);let x="/api/Statistical",g=e=>f.e.get(x+"/CustomerOverview",{params:e}),v=e=>f.e.get(x+"/CustomerCompareOverview",{params:e}),y=e=>f.e.get(x+"/NewCustomer12Month",{params:e}),b=e=>f.e.get(x+"/ConversionRateStatistics",{params:e}),j=e=>f.e.get(x+"/CustomerByLearningNeed",{params:e}),w=e=>f.e.get(x+"/CustomerByLearningPurpose",{params:e}),N=e=>f.e.get(x+"/CustomerBySource",{params:e});var _=a(75499),Y=()=>{var e,t,a,n;let r=(0,m.v9)(e=>e.user.currentBranch),[s,o]=(0,i.useState)(c()()),{data:u,isLoading:f,isFetching:x}=(0,p.a)({queryKey:["CUSTOMER-OVERVIEW",r,s],queryFn:()=>g({branchIds:r,month:Number(c()(s).format("MM")),year:Number(c()(s).format("YYYYY"))}).then(e=>e.data),enabled:!!r&&!!s,staleTime:1/0}),{data:y,isLoading:b,isFetching:j}=(0,p.a)({queryKey:["CUSTOMER-COMPARE",r,s],queryFn:()=>v({branchIds:r,month:Number(c()(s).format("MM")),year:Number(c()(s).format("YYYYY"))}).then(e=>e.data),enabled:!!r&&!!s,staleTime:1/0});return(0,l.jsxs)("div",{children:[((null==u?void 0:null===(e=u.data)||void 0===e?void 0:e.length)>0||(null==y?void 0:null===(t=y.data)||void 0===t?void 0:t.length)>0)&&(0,l.jsx)(d.default,{className:"primary-input mb-[10px]",picker:"month",placeholder:"Chọn th\xe1ng",defaultValue:s,format:"MMM YYYY",allowClear:!1,onChange:e=>{o(e)}}),(0,l.jsxs)("div",{className:"grid gap-3 w1400:grid-cols-3 w700:grid-cols-2 grid-cols-1",children:[x&&(0,l.jsx)(l.Fragment,{children:(0,l.jsx)(_.Z,{})}),j&&(0,l.jsxs)(l.Fragment,{children:[(0,l.jsx)(_.Z,{}),(0,l.jsx)(_.Z,{}),(0,l.jsx)(_.Z,{}),(0,l.jsx)(_.Z,{}),(0,l.jsx)(_.Z,{})]}),!!u&&!x&&(null===(a=u.data)||void 0===a?void 0:a.map(e=>(0,l.jsx)(h.Z,{title:null==e?void 0:e.Type,status:4,value:null==e?void 0:e.Value}))),!!y&&!j&&(null===(n=y.data)||void 0===n?void 0:n.map(e=>(0,l.jsx)(h.Z,{title:null==e?void 0:e.Type,status:null==e?void 0:e.Status,value:null==e?void 0:e.Value,diffValue:null==e?void 0:e.DifferenceValue,detailDifference:null==e?void 0:e.DifferenceQuantity,lastMonth:c()(s).subtract(1,"months").format("MM/YYYY")})))]})]})},T=a(44011),Z=a(55330),C=a(36693),F=()=>{let e;let t=(0,m.v9)(e=>e.user.currentBranch),[a,n]=(0,i.useState)(c()()),{isMobile:r}=(0,C.Z)(),{data:s,isLoading:o,isFetching:u,refetch:h}=(0,p.a)({queryKey:["ConversionRateStatistics",t,a],queryFn:()=>b({branchIds:t,month:Number(c()(a).format("MM")),year:Number(c()(a).format("YYYYY"))}).then(e=>e.data),enabled:!!t&&!!a,staleTime:1/0});return(0,l.jsx)(Z.Z,{title:"Biểu đồ ph\xe2n t\xedch trạng th\xe1i kh\xe1ch h\xe0ng",subTitle:"Dữ liệu tỉ lệ chuyển đổi theo trạng th\xe1i kh\xe1ch h\xe0ng",extra:(0,l.jsx)(d.default,{disabled:o||u,picker:"month",format:"MM/YYYY",defaultValue:c()(),className:"primary-input w-[110px]",onChange:e=>{n(e)},allowClear:!1}),children:(0,l.jsx)(T.Z,{loading:o||u,legendPosition:r?"bottom":"right",angleField:"value",colorField:"type",isDonut:!0,className:"h-[335px]",isPercent:!1,data:s?null==(e=s.data)?void 0:e.map(e=>({type:null==e?void 0:e.Type,value:e.Value})):[]})})},M=a(68973),P=a(66734),k=()=>{let e=(0,m.v9)(e=>e.user.currentBranch),[t,a]=(0,i.useState)(c()()),{data:n,isLoading:r,isFetching:s}=(0,p.a)({queryKey:["NewCustomer12Month",e,t],queryFn:()=>y({branchIds:e,month:Number(c()(t).format("MM")),year:Number(c()(t).format("YYYYY"))}).then(e=>e.data),enabled:!!e&&!!t,staleTime:1/0});return(0,l.jsx)(Z.Z,{title:"Kh\xe1ch h\xe0ng mới",subTitle:"Số lượng kh\xe1ch h\xe0ng mới qua từng th\xe1ng",extra:(0,l.jsx)(d.default,{picker:"year",defaultValue:c()(),className:"primary-input w-[100px]",allowClear:!1,onChange:e=>{a(e)}}),children:(0,l.jsx)(M.Z,{color:["#338BF1","#F7401A"],loading:r||s,data:n?(0,P.T)(n.data):[],xField:"month",yField:"value",seriesField:"type",haveSlider:!1,smooth:!0})})},D=()=>{let e=(0,m.v9)(e=>e.user.currentBranch),[t,a]=(0,i.useState)(c()()),{isMobile:n}=(0,C.Z)(),{data:r,isLoading:s,isFetching:o}=(0,p.a)({queryKey:["CustomerByLearningNeed",e,t],queryFn:()=>j({branchIds:e,month:Number(c()(t).format("MM")),year:Number(c()(t).format("YYYYY"))}).then(e=>e.data),enabled:!!e&&!!t,staleTime:1/0});return(0,l.jsx)(Z.Z,{title:"Tỉ lệ nhu cầu học",subTitle:"Dữ liệu tỉ lệ nhu cầu học của học vi\xean",extra:(0,l.jsx)(d.default,{disabled:o||s,picker:"month",format:"MM/YYYY",defaultValue:c()(),className:"primary-input w-[110px]",allowClear:!1,onChange:e=>{a(e)}}),children:(0,l.jsx)(T.Z,{loading:s||o,legendPosition:n?"bottom":"right",angleField:"value",colorField:"type",isDonut:!1,isPercent:!1,data:r?(0,P.l)(r.data):[]})})},S=()=>{let e=(0,m.v9)(e=>e.user.currentBranch),[t,a]=(0,i.useState)(c()()),{isMobile:n}=(0,C.Z)(),{data:r,isLoading:s,isFetching:o}=(0,p.a)({queryKey:["CustomerByLearningPurpose",e,t],queryFn:()=>w({branchIds:e,month:Number(c()(t).format("MM")),year:Number(c()(t).format("YYYYY"))}).then(e=>e.data),enabled:!!e&&!!t,staleTime:1/0});return(0,l.jsx)(Z.Z,{title:"Tỉ lệ mục đ\xedch học",subTitle:"Dữ liệu tỉ lệ mục đ\xedch học của học vi\xean",extra:(0,l.jsx)(d.default,{disabled:s||o,picker:"month",format:"MM/YYYY",defaultValue:c()(),className:"primary-input w-[110px]",allowClear:!1,onChange:e=>{a(e)}}),children:(0,l.jsx)(T.Z,{loading:s||o,legendPosition:n?"bottom":"right",angleField:"value",colorField:"type",isDonut:!1,isPercent:!1,data:r?(0,P.l)(r.data):[]})})},E=()=>{let e=(0,m.v9)(e=>e.user.currentBranch),[t,a]=(0,i.useState)(c()()),[n,r]=(0,i.useState)({pageIndex:1,pageSize:10}),{isMobile:s}=(0,C.Z)(),{data:o,isLoading:u,isFetching:h,refetch:f}=(0,p.a)({queryKey:["RevenueByStudent",e,t,n.pageIndex],queryFn:()=>N({branchIds:e,month:Number(c()(t).format("MM")),year:Number(c()(t).format("YYYYY")),...n}).then(e=>e.data),enabled:!!e&&!!t,staleTime:1/0});return(0,l.jsx)(Z.Z,{title:"Tỉ lệ nguồn kh\xe1ch h\xe0ng",subTitle:"Dữ liệu tỉ lệ nguồn kh\xe1ch h\xe0ng",extra:(0,l.jsx)(d.default,{disabled:u||h,picker:"month",format:"MM/YYYY",defaultValue:c()(),className:"primary-input w-[110px]",allowClear:!1,onChange:e=>{a(e)}}),children:(0,l.jsx)(T.Z,{loading:!1,legendPosition:s?"bottom":"right",angleField:"value",colorField:"type",isDonut:!1,isPercent:!1,data:o?(0,P.l)(o.data):[]})})},O=()=>(0,l.jsxs)("div",{className:"font-inter",children:[(0,l.jsx)("div",{className:"mb-3",children:(0,l.jsx)(Y,{})}),(0,l.jsxs)("div",{className:"grid gap-3 w1400:grid-cols-8 w700:grid-cols-2 grid-cols-1 mb-3",children:[(0,l.jsx)("div",{className:"w1400:col-span-5 col-span-2",children:(0,l.jsx)(k,{})}),(0,l.jsx)("div",{className:"w1400:col-span-3 col-span-2 h-full",children:(0,l.jsx)(F,{})})]}),(0,l.jsxs)("div",{className:"grid gap-3 w1400:grid-cols-3 w700:grid-cols-2 grid-cols-1 mb-3",children:[(0,l.jsx)(D,{}),(0,l.jsx)(S,{}),(0,l.jsx)(E,{})]})]});let V=()=>(0,l.jsxs)(l.Fragment,{children:[(0,l.jsx)(r(),{children:(0,l.jsxs)("title",{children:[s.Z.appName," - Thống k\xea kh\xe1ch h\xe0ng"]})}),(0,l.jsx)(O,{})]});V.Layout=o.Zn;var L=V},41622:function(e){e.exports={arrowUp:"styles_arrowUp__DQdSF",arrowDown:"styles_arrowDown__cgqyB",chartUp:"styles_chartUp__hUvCH",chartDown:"styles_chartDown__Zljb_"}},5152:function(e,t,a){e.exports=a(28864)}},function(e){e.O(0,[6130,4838,7909,8391,5970,6660,4396,3365,4321,2961,4738,8460,9915,6565,653,6655,1607,6361,5009,9872,3604,6369,6954,391,2888,9774,179],function(){return e(e.s=80268)}),_N_E=e.O()}]);