"use strict";(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[1607],{64921:function(e,n){n.Z={icon:{tag:"svg",attrs:{viewBox:"64 64 896 896",focusable:"false"},children:[{tag:"path",attrs:{d:"M176 511a56 56 0 10112 0 56 56 0 10-112 0zm280 0a56 56 0 10112 0 56 56 0 10-112 0zm280 0a56 56 0 10112 0 56 56 0 10-112 0z"}}]},name:"ellipsis",theme:"outlined"}},13481:function(e,n,t){t.d(n,{Z:function(){return i}});var r=t(97685),o=t(67294);function i(){var e=o.useReducer(function(e){return e+1},0);return(0,r.Z)(e,2)[1]}},8994:function(e,n,t){var r=t(67294),o=t(13481),i=t(61210);n.Z=function(){var e=!(arguments.length>0)||void 0===arguments[0]||arguments[0],n=(0,r.useRef)({}),t=(0,o.Z)();return(0,r.useEffect)(function(){var r=i.ZP.subscribe(function(r){n.current=r,e&&t()});return function(){return i.ZP.unsubscribe(r)}},[]),n.current}},77889:function(e,n,t){t.d(n,{u:function(){return m},Z:function(){return g}});var r=t(87462),o=t(4942),i=t(97685),l=t(93967),a=t.n(l),u=t(74533),c=t(67294),s=t(93565),f=t(47887);function d(e){var n=e.className,t=e.direction,i=e.index,l=e.marginDirection,a=e.children,u=e.split,s=e.wrap,f=c.useContext(m),d=f.horizontalSize,p=f.verticalSize,v=f.latestIndex,y=f.supportFlexGap,h={};return(!y&&("vertical"===t?i<v&&(h={marginBottom:d/(u?2:1)}):h=(0,r.Z)((0,r.Z)({},i<v&&(0,o.Z)({},l,d/(u?2:1))),s&&{paddingBottom:p})),null==a)?null:c.createElement(c.Fragment,null,c.createElement("div",{className:n,style:h},a),i<v&&u&&c.createElement("span",{className:"".concat(n,"-split"),style:h},u))}var p=t(59661),v=function(e,n){var t={};for(var r in e)Object.prototype.hasOwnProperty.call(e,r)&&0>n.indexOf(r)&&(t[r]=e[r]);if(null!=e&&"function"==typeof Object.getOwnPropertySymbols)for(var o=0,r=Object.getOwnPropertySymbols(e);o<r.length;o++)0>n.indexOf(r[o])&&Object.prototype.propertyIsEnumerable.call(e,r[o])&&(t[r[o]]=e[r[o]]);return t},m=c.createContext({latestIndex:0,horizontalSize:0,verticalSize:0,supportFlexGap:!1}),y={small:8,middle:16,large:24},h=function(e){var n=c.useContext(s.E_),t=n.getPrefixCls,l=n.space,p=n.direction,h=e.size,g=void 0===h?(null==l?void 0:l.size)||"small":h,Z=e.align,b=e.className,C=e.children,E=e.direction,w=void 0===E?"horizontal":E,x=e.prefixCls,N=e.split,P=e.style,S=e.wrap,M=void 0!==S&&S,k=v(e,["size","align","className","children","direction","prefixCls","split","style","wrap"]),I=(0,f.Z)(),R=c.useMemo(function(){return(Array.isArray(g)?g:[g,g]).map(function(e){return"string"==typeof e?y[e]:e||0})},[g]),K=(0,i.Z)(R,2),O=K[0],A=K[1],T=(0,u.Z)(C,{keepEmpty:!0}),D=void 0===Z&&"horizontal"===w?"center":Z,L=t("space",x),z=a()(L,"".concat(L,"-").concat(w),(0,o.Z)((0,o.Z)({},"".concat(L,"-rtl"),"rtl"===p),"".concat(L,"-align-").concat(D),D),b),_="".concat(L,"-item"),V="rtl"===p?"marginLeft":"marginRight",F=0,j=T.map(function(e,n){null!=e&&(F=n);var t=e&&e.key||"".concat(_,"-").concat(n);return c.createElement(d,{className:_,key:t,direction:w,index:n,marginDirection:V,split:N,wrap:M},e)}),W=c.useMemo(function(){return{horizontalSize:O,verticalSize:A,latestIndex:F,supportFlexGap:I}},[O,A,F,I]);if(0===T.length)return null;var G={};return M&&(G.flexWrap="wrap",I||(G.marginBottom=-A)),I&&(G.columnGap=O,G.rowGap=A),c.createElement("div",(0,r.Z)({className:z,style:(0,r.Z)((0,r.Z)({},G),P)},k),c.createElement(m.Provider,{value:W},j))};h.Compact=p.ZP;var g=h},48014:function(e,n,t){var r=t(1413),o=t(67294),i=t(64921),l=t(28870),a=o.forwardRef(function(e,n){return o.createElement(l.Z,(0,r.Z)((0,r.Z)({},e),{},{ref:n,icon:i.Z}))});n.Z=a},84081:function(e,n,t){t.d(n,{tS:function(){return l}});var r=t(74902),o=t(85004);function i(e){var n=arguments.length>1&&void 0!==arguments[1]&&arguments[1];if((0,o.Z)(e)){var t=e.nodeName.toLowerCase(),r=["input","select","textarea","button"].includes(t)||e.isContentEditable||"a"===t&&!!e.getAttribute("href"),i=e.getAttribute("tabindex"),l=Number(i),a=null;return i&&!Number.isNaN(l)?a=l:r&&null===a&&(a=0),r&&e.disabled&&(a=null),null!==a&&(a>=0||n&&a<0)}return!1}function l(e){var n=arguments.length>1&&void 0!==arguments[1]&&arguments[1],t=(0,r.Z)(e.querySelectorAll("*")).filter(function(e){return i(e,n)});return i(e,n)&&t.unshift(e),t}},60057:function(e,n,t){t.d(n,{Z:function(){return b}});var r=t(4942),o=t(1413),i=t(97685),l=t(45987),a=t(67294),u=t(79498),c=t(93967),s=t.n(c),f={adjustX:1,adjustY:1},d=[0,0],p={topLeft:{points:["bl","tl"],overflow:f,offset:[0,-4],targetOffset:d},topCenter:{points:["bc","tc"],overflow:f,offset:[0,-4],targetOffset:d},topRight:{points:["br","tr"],overflow:f,offset:[0,-4],targetOffset:d},bottomLeft:{points:["tl","bl"],overflow:f,offset:[0,4],targetOffset:d},bottomCenter:{points:["tc","bc"],overflow:f,offset:[0,4],targetOffset:d},bottomRight:{points:["tr","br"],overflow:f,offset:[0,4],targetOffset:d}},v=t(28778),m=t(84184),y=t(84081),h=v.Z.ESC,g=v.Z.TAB,Z=["arrow","prefixCls","transitionName","animation","align","placement","placements","getPopupContainer","showAction","hideAction","overlayClassName","overlayStyle","visible","trigger","autoFocus"],b=a.forwardRef(function(e,n){var t,c,f,d,v,b,C,E,w,x,N,P,S,M,k,I,R=e.arrow,K=void 0!==R&&R,O=e.prefixCls,A=void 0===O?"rc-dropdown":O,T=e.transitionName,D=e.animation,L=e.align,z=e.placement,_=e.placements,V=e.getPopupContainer,F=e.showAction,j=e.hideAction,W=e.overlayClassName,G=e.overlayStyle,B=e.visible,H=e.trigger,q=void 0===H?["hover"]:H,X=e.autoFocus,U=(0,l.Z)(e,Z),Y=a.useState(),J=(0,i.Z)(Y,2),Q=J[0],$=J[1],ee="visible"in e?B:Q,en=a.useRef(null);a.useImperativeHandle(n,function(){return en.current}),f=(c={visible:ee,setTriggerVisible:$,triggerRef:en,onVisibleChange:e.onVisibleChange,autoFocus:X}).visible,d=c.setTriggerVisible,v=c.triggerRef,b=c.onVisibleChange,C=c.autoFocus,E=a.useRef(!1),w=function(){if(f&&v.current){var e,n,t,r;null===(e=v.current)||void 0===e||null===(n=e.triggerRef)||void 0===n||null===(t=n.current)||void 0===t||null===(r=t.focus)||void 0===r||r.call(t),d(!1),"function"==typeof b&&b(!1)}},x=function(){var e,n,t,r,o=(0,y.tS)(null===(e=v.current)||void 0===e?void 0:null===(n=e.popupRef)||void 0===n?void 0:null===(t=n.current)||void 0===t?void 0:null===(r=t.getElement)||void 0===r?void 0:r.call(t))[0];return null!=o&&!!o.focus&&(o.focus(),E.current=!0,!0)},N=function(e){switch(e.keyCode){case h:w();break;case g:var n=!1;E.current||(n=x()),n?e.preventDefault():w()}},a.useEffect(function(){return f?(window.addEventListener("keydown",N),C&&(0,m.Z)(x,3),function(){window.removeEventListener("keydown",N),E.current=!1}):function(){E.current=!1}},[f]);var et=function(){var n=e.overlay;return"function"==typeof n?n():n},er=function(){var e=et();return a.createElement(a.Fragment,null,K&&a.createElement("div",{className:"".concat(A,"-arrow")}),e)},eo=j;return eo||-1===q.indexOf("contextMenu")||(eo=["click"]),a.createElement(u.Z,(0,o.Z)((0,o.Z)({builtinPlacements:void 0===_?p:_},U),{},{prefixCls:A,ref:en,popupClassName:s()(W,(0,r.Z)({},"".concat(A,"-show-arrow"),K)),popupStyle:G,action:q,showAction:F,hideAction:eo||[],popupPlacement:void 0===z?"bottomLeft":z,popupAlign:L,popupTransitionName:T,popupAnimation:D,popupVisible:ee,stretch:(P=e.minOverlayWidthMatchTrigger,S=e.alignPoint,"minOverlayWidthMatchTrigger"in e?P:!S)?"minWidth":"",popup:"function"==typeof e.overlay?er:er(),onPopupVisibleChange:function(n){var t=e.onVisibleChange;$(n),"function"==typeof t&&t(n)},onPopupClick:function(n){var t=e.onOverlayClick;$(!1),t&&t(n)},getPopupContainer:V}),(k=(M=e.children).props?M.props:{},I=s()(k.className,void 0!==(t=e.openClassName)?t:"".concat(A,"-open")),ee&&M?a.cloneElement(M,{className:I}):M))})},97868:function(e,n,t){t.d(n,{iz:function(){return e_},ck:function(){return ed},BW:function(){return ez},sN:function(){return ed},GP:function(){return ez},Wd:function(){return eR},ZP:function(){return eV},Xl:function(){return S}});var r=t(87462),o=t(4942),i=t(1413),l=t(74902),a=t(97685),u=t(45987),c=t(93967),s=t.n(c),f=t(39983),d=t(54043),p=t(10341),v=t(67294),m=t(73935),y=t(39046),h=v.createContext(null);function g(e,n){return void 0===e?null:"".concat(e,"-").concat(n)}function Z(e){return g(v.useContext(h),e)}var b=t(33325),C=["children","locked"],E=v.createContext(null);function w(e){var n=e.children,t=e.locked,r=(0,u.Z)(e,C),o=v.useContext(E),l=(0,b.Z)(function(){var e;return e=(0,i.Z)({},o),Object.keys(r).forEach(function(n){var t=r[n];void 0!==t&&(e[n]=t)}),e},[o,r],function(e,n){return!t&&(e[0]!==n[0]||!(0,y.Z)(e[1],n[1],!0))});return v.createElement(E.Provider,{value:l},n)}var x=v.createContext(null);function N(){return v.useContext(x)}var P=v.createContext([]);function S(e){var n=v.useContext(P);return v.useMemo(function(){return void 0!==e?[].concat((0,l.Z)(n),[e]):n},[n,e])}var M=v.createContext(null),k=v.createContext({}),I=t(28778),R=t(84184),K=t(84081),O=I.Z.LEFT,A=I.Z.RIGHT,T=I.Z.UP,D=I.Z.DOWN,L=I.Z.ENTER,z=I.Z.ESC,_=I.Z.HOME,V=I.Z.END,F=[T,D,O,A];function j(e,n){return(0,K.tS)(e,!0).filter(function(e){return n.has(e)})}function W(e,n,t){var r=arguments.length>3&&void 0!==arguments[3]?arguments[3]:1;if(!e)return null;var o=j(e,n),i=o.length,l=o.findIndex(function(e){return t===e});return r<0?-1===l?l=i-1:l-=1:r>0&&(l+=1),o[l=(l+i)%i]}var G="__RC_UTIL_PATH_SPLIT__",B=function(e){return e.join(G)},H="rc-menu-more";function q(e){var n=v.useRef(e);n.current=e;var t=v.useCallback(function(){for(var e,t=arguments.length,r=Array(t),o=0;o<t;o++)r[o]=arguments[o];return null===(e=n.current)||void 0===e?void 0:e.call.apply(e,[n].concat(r))},[]);return e?t:void 0}var X=Math.random().toFixed(5).toString().slice(2),U=0,Y=t(15671),J=t(43144),Q=t(60136),$=t(29388),ee=t(25084),en=t(91463);function et(e,n,t,r){var o=v.useContext(E),i=o.activeKey,l=o.onActive,a=o.onInactive,u={active:i===e};return n||(u.onMouseEnter=function(n){null==t||t({key:e,domEvent:n}),l(e)},u.onMouseLeave=function(n){null==r||r({key:e,domEvent:n}),a(e)}),u}function er(e){var n=v.useContext(E),t=n.mode,r=n.rtl,o=n.inlineIndent;return"inline"!==t?null:r?{paddingRight:e*o}:{paddingLeft:e*o}}function eo(e){var n=e.icon,t=e.props,r=e.children;return("function"==typeof n?v.createElement(n,(0,i.Z)({},t)):n)||r||null}var ei=["item"];function el(e){var n=e.item,t=(0,u.Z)(e,ei);return Object.defineProperty(t,"item",{get:function(){return(0,p.ZP)(!1,"`info.item` is deprecated since we will move to function component that not provides React Node instance in future."),n}}),t}var ea=["title","attribute","elementRef"],eu=["style","className","eventKey","warnKey","disabled","itemIcon","children","role","onMouseEnter","onMouseLeave","onClick","onKeyDown","onFocus"],ec=["active"],es=function(e){(0,Q.Z)(t,e);var n=(0,$.Z)(t);function t(){return(0,Y.Z)(this,t),n.apply(this,arguments)}return(0,J.Z)(t,[{key:"render",value:function(){var e=this.props,n=e.title,t=e.attribute,o=e.elementRef,i=(0,u.Z)(e,ea),l=(0,ee.Z)(i,["eventKey","popupClassName","popupOffset","onTitleClick"]);return(0,p.ZP)(!t,"`attribute` of Menu.Item is deprecated. Please pass attribute directly."),v.createElement(f.Z.Item,(0,r.Z)({},t,{title:"string"==typeof n?n:void 0},l,{ref:o}))}}]),t}(v.Component),ef=v.forwardRef(function(e,n){var t,a=e.style,c=e.className,f=e.eventKey,d=(e.warnKey,e.disabled),p=e.itemIcon,m=e.children,y=e.role,h=e.onMouseEnter,g=e.onMouseLeave,b=e.onClick,C=e.onKeyDown,w=e.onFocus,x=(0,u.Z)(e,eu),N=Z(f),P=v.useContext(E),M=P.prefixCls,R=P.onItemClick,K=P.disabled,O=P.overflowDisabled,A=P.itemIcon,T=P.selectedKeys,D=P.onActive,L=v.useContext(k)._internalRenderMenuItem,z="".concat(M,"-item"),_=v.useRef(),V=v.useRef(),F=K||d,j=(0,en.x1)(n,V),W=S(f),G=function(e){return{key:f,keyPath:(0,l.Z)(W).reverse(),item:_.current,domEvent:e}},B=et(f,F,h,g),H=B.active,q=(0,u.Z)(B,ec),X=T.includes(f),U=er(W.length),Y={};"option"===e.role&&(Y["aria-selected"]=X);var J=v.createElement(es,(0,r.Z)({ref:_,elementRef:j,role:null===y?"none":y||"menuitem",tabIndex:d?null:-1,"data-menu-id":O&&N?null:N},x,q,Y,{component:"li","aria-disabled":d,style:(0,i.Z)((0,i.Z)({},U),a),className:s()(z,(t={},(0,o.Z)(t,"".concat(z,"-active"),H),(0,o.Z)(t,"".concat(z,"-selected"),X),(0,o.Z)(t,"".concat(z,"-disabled"),F),t),c),onClick:function(e){if(!F){var n=G(e);null==b||b(el(n)),R(n)}},onKeyDown:function(e){if(null==C||C(e),e.which===I.Z.ENTER){var n=G(e);null==b||b(el(n)),R(n)}},onFocus:function(e){D(f),null==w||w(e)}}),m,v.createElement(eo,{props:(0,i.Z)((0,i.Z)({},e),{},{isSelected:X}),icon:p||A}));return L&&(J=L(J,e,{selected:X})),J}),ed=v.forwardRef(function(e,n){var t=e.eventKey,o=N(),i=S(t);return(v.useEffect(function(){if(o)return o.registerPath(t,i),function(){o.unregisterPath(t,i)}},[i]),o)?null:v.createElement(ef,(0,r.Z)({},e,{ref:n}))}),ep=["className","children"],ev=v.forwardRef(function(e,n){var t=e.className,o=e.children,i=(0,u.Z)(e,ep),l=v.useContext(E),a=l.prefixCls,c=l.mode,f=l.rtl;return v.createElement("ul",(0,r.Z)({className:s()(a,f&&"".concat(a,"-rtl"),"".concat(a,"-sub"),"".concat(a,"-").concat("inline"===c?"inline":"vertical"),t),role:"menu"},i,{"data-menu-list":!0,ref:n}),o)});ev.displayName="SubMenuList";var em=t(71002),ey=t(74533),eh=["label","children","key","type"];function eg(e,n){return(0,ey.Z)(e).map(function(e,t){if(v.isValidElement(e)){var r,o,i=e.key,a=null!==(r=null===(o=e.props)||void 0===o?void 0:o.eventKey)&&void 0!==r?r:i;null==a&&(a="tmp_key-".concat([].concat((0,l.Z)(n),[t]).join("-")));var u={key:a,eventKey:a};return v.cloneElement(e,u)}return e})}var eZ=t(79498),eb={adjustX:1,adjustY:1},eC={topLeft:{points:["bl","tl"],overflow:eb,offset:[0,-7]},bottomLeft:{points:["tl","bl"],overflow:eb,offset:[0,7]},leftTop:{points:["tr","tl"],overflow:eb,offset:[-4,0]},rightTop:{points:["tl","tr"],overflow:eb,offset:[4,0]}},eE={topLeft:{points:["bl","tl"],overflow:eb,offset:[0,-7]},bottomLeft:{points:["tl","bl"],overflow:eb,offset:[0,7]},rightTop:{points:["tr","tl"],overflow:eb,offset:[-4,0]},leftTop:{points:["tl","tr"],overflow:eb,offset:[4,0]}};function ew(e,n,t){return n||(t?t[e]||t.other:void 0)}var ex={horizontal:"bottomLeft",vertical:"rightTop","vertical-left":"rightTop","vertical-right":"leftTop"};function eN(e){var n=e.prefixCls,t=e.visible,r=e.children,l=e.popup,u=e.popupClassName,c=e.popupOffset,f=e.disabled,d=e.mode,p=e.onVisibleChange,m=v.useContext(E),y=m.getPopupContainer,h=m.rtl,g=m.subMenuOpenDelay,Z=m.subMenuCloseDelay,b=m.builtinPlacements,C=m.triggerSubMenuAction,w=m.forceSubMenuRender,x=m.rootClassName,N=m.motion,P=m.defaultMotions,S=v.useState(!1),M=(0,a.Z)(S,2),k=M[0],I=M[1],K=h?(0,i.Z)((0,i.Z)({},eE),b):(0,i.Z)((0,i.Z)({},eC),b),O=ex[d],A=ew(d,N,P),T=v.useRef(A);"inline"!==d&&(T.current=A);var D=(0,i.Z)((0,i.Z)({},T.current),{},{leavedClassName:"".concat(n,"-hidden"),removeOnLeave:!1,motionAppear:!0}),L=v.useRef();return v.useEffect(function(){return L.current=(0,R.Z)(function(){I(t)}),function(){R.Z.cancel(L.current)}},[t]),v.createElement(eZ.Z,{prefixCls:n,popupClassName:s()("".concat(n,"-popup"),(0,o.Z)({},"".concat(n,"-rtl"),h),u,x),stretch:"horizontal"===d?"minWidth":null,getPopupContainer:y,builtinPlacements:K,popupPlacement:O,popupVisible:k,popup:l,popupAlign:c&&{offset:c},action:f?[]:[C],mouseEnterDelay:g,mouseLeaveDelay:Z,onPopupVisibleChange:p,forceRender:w,popupMotion:D},r)}var eP=t(82225);function eS(e){var n=e.id,t=e.open,o=e.keyPath,l=e.children,u="inline",c=v.useContext(E),s=c.prefixCls,f=c.forceSubMenuRender,d=c.motion,p=c.defaultMotions,m=c.mode,y=v.useRef(!1);y.current=m===u;var h=v.useState(!y.current),g=(0,a.Z)(h,2),Z=g[0],b=g[1],C=!!y.current&&t;v.useEffect(function(){y.current&&b(!1)},[m]);var x=(0,i.Z)({},ew(u,d,p));o.length>1&&(x.motionAppear=!1);var N=x.onVisibleChanged;return(x.onVisibleChanged=function(e){return y.current||e||b(!0),null==N?void 0:N(e)},Z)?null:v.createElement(w,{mode:u,locked:!y.current},v.createElement(eP.default,(0,r.Z)({visible:C},x,{forceRender:f,removeOnLeave:!1,leavedClassName:"".concat(s,"-hidden")}),function(e){var t=e.className,r=e.style;return v.createElement(ev,{id:n,className:t,style:r},l)}))}var eM=["style","className","title","eventKey","warnKey","disabled","internalPopupClose","children","itemIcon","expandIcon","popupClassName","popupOffset","onClick","onMouseEnter","onMouseLeave","onTitleClick","onTitleMouseEnter","onTitleMouseLeave"],ek=["active"],eI=function(e){var n,t=e.style,l=e.className,c=e.title,d=e.eventKey,p=(e.warnKey,e.disabled),m=e.internalPopupClose,y=e.children,h=e.itemIcon,g=e.expandIcon,b=e.popupClassName,C=e.popupOffset,x=e.onClick,N=e.onMouseEnter,P=e.onMouseLeave,I=e.onTitleClick,R=e.onTitleMouseEnter,K=e.onTitleMouseLeave,O=(0,u.Z)(e,eM),A=Z(d),T=v.useContext(E),D=T.prefixCls,L=T.mode,z=T.openKeys,_=T.disabled,V=T.overflowDisabled,F=T.activeKey,j=T.selectedKeys,W=T.itemIcon,G=T.expandIcon,B=T.onItemClick,H=T.onOpenChange,X=T.onActive,U=v.useContext(k)._internalRenderSubMenuItem,Y=v.useContext(M).isSubPathKey,J=S(),Q="".concat(D,"-submenu"),$=_||p,ee=v.useRef(),en=v.useRef(),ei=g||G,ea=z.includes(d),eu=!V&&ea,ec=Y(j,d),es=et(d,$,R,K),ef=es.active,ed=(0,u.Z)(es,ek),ep=v.useState(!1),em=(0,a.Z)(ep,2),ey=em[0],eh=em[1],eg=function(e){$||eh(e)},eZ=v.useMemo(function(){return ef||"inline"!==L&&(ey||Y([F],d))},[L,ef,F,ey,d,Y]),eb=er(J.length),eC=q(function(e){null==x||x(el(e)),B(e)}),eE=A&&"".concat(A,"-popup"),ew=v.createElement("div",(0,r.Z)({role:"menuitem",style:eb,className:"".concat(Q,"-title"),tabIndex:$?null:-1,ref:ee,title:"string"==typeof c?c:null,"data-menu-id":V&&A?null:A,"aria-expanded":eu,"aria-haspopup":!0,"aria-controls":eE,"aria-disabled":$,onClick:function(e){$||(null==I||I({key:d,domEvent:e}),"inline"===L&&H(d,!ea))},onFocus:function(){X(d)}},ed),c,v.createElement(eo,{icon:"horizontal"!==L?ei:null,props:(0,i.Z)((0,i.Z)({},e),{},{isOpen:eu,isSubMenu:!0})},v.createElement("i",{className:"".concat(Q,"-arrow")}))),ex=v.useRef(L);if("inline"!==L&&J.length>1?ex.current="vertical":ex.current=L,!V){var eP=ex.current;ew=v.createElement(eN,{mode:eP,prefixCls:Q,visible:!m&&eu&&"inline"!==L,popupClassName:b,popupOffset:C,popup:v.createElement(w,{mode:"horizontal"===eP?"vertical":eP},v.createElement(ev,{id:eE,ref:en},y)),disabled:$,onVisibleChange:function(e){"inline"!==L&&H(d,e)}},ew)}var eI=v.createElement(f.Z.Item,(0,r.Z)({role:"none"},O,{component:"li",style:t,className:s()(Q,"".concat(Q,"-").concat(L),l,(n={},(0,o.Z)(n,"".concat(Q,"-open"),eu),(0,o.Z)(n,"".concat(Q,"-active"),eZ),(0,o.Z)(n,"".concat(Q,"-selected"),ec),(0,o.Z)(n,"".concat(Q,"-disabled"),$),n)),onMouseEnter:function(e){eg(!0),null==N||N({key:d,domEvent:e})},onMouseLeave:function(e){eg(!1),null==P||P({key:d,domEvent:e})}}),ew,!V&&v.createElement(eS,{id:eE,open:eu,keyPath:J},y));return U&&(eI=U(eI,e,{selected:ec,active:eZ,open:eu,disabled:$})),v.createElement(w,{onItemClick:eC,mode:"horizontal"===L?"vertical":L,itemIcon:h||W,expandIcon:ei},eI)};function eR(e){var n,t=e.eventKey,r=e.children,o=S(t),i=eg(r,o),l=N();return v.useEffect(function(){if(l)return l.registerPath(t,o),function(){l.unregisterPath(t,o)}},[o]),n=l?i:v.createElement(eI,e,i),v.createElement(P.Provider,{value:o},n)}var eK=["prefixCls","rootClassName","style","className","tabIndex","items","children","direction","id","mode","inlineCollapsed","disabled","disabledOverflow","subMenuOpenDelay","subMenuCloseDelay","forceSubMenuRender","defaultOpenKeys","openKeys","activeKey","defaultActiveFirst","selectable","multiple","defaultSelectedKeys","selectedKeys","onSelect","onDeselect","inlineIndent","motion","defaultMotions","triggerSubMenuAction","builtinPlacements","itemIcon","expandIcon","overflowedIndicator","overflowedIndicatorPopupClassName","getPopupContainer","onClick","onOpenChange","onKeyDown","openAnimation","openTransitionName","_internalRenderMenuItem","_internalRenderSubMenuItem"],eO=[],eA=v.forwardRef(function(e,n){var t,c,p,Z,b,C,E,N,P,S,I,K,Y,J,Q,$,ee,en,et,er,eo,ei,ea,eu,ec,es,ef,ep=e.prefixCls,ev=void 0===ep?"rc-menu":ep,ey=e.rootClassName,eZ=e.style,eb=e.className,eC=e.tabIndex,eE=e.items,ew=e.children,ex=e.direction,eN=e.id,eP=e.mode,eS=void 0===eP?"vertical":eP,eM=e.inlineCollapsed,ek=e.disabled,eI=e.disabledOverflow,eA=e.subMenuOpenDelay,eT=e.subMenuCloseDelay,eD=e.forceSubMenuRender,eL=e.defaultOpenKeys,eV=e.openKeys,eF=e.activeKey,ej=e.defaultActiveFirst,eW=e.selectable,eG=void 0===eW||eW,eB=e.multiple,eH=void 0!==eB&&eB,eq=e.defaultSelectedKeys,eX=e.selectedKeys,eU=e.onSelect,eY=e.onDeselect,eJ=e.inlineIndent,eQ=e.motion,e$=e.defaultMotions,e0=e.triggerSubMenuAction,e1=e.builtinPlacements,e4=e.itemIcon,e2=e.expandIcon,e6=e.overflowedIndicator,e8=void 0===e6?"...":e6,e7=e.overflowedIndicatorPopupClassName,e9=e.getPopupContainer,e5=e.onClick,e3=e.onOpenChange,ne=e.onKeyDown,nn=(e.openAnimation,e.openTransitionName,e._internalRenderMenuItem),nt=e._internalRenderSubMenuItem,nr=(0,u.Z)(e,eK),no=v.useMemo(function(){var e;return e=ew,eE&&(e=function e(n){return(n||[]).map(function(n,t){if(n&&"object"===(0,em.Z)(n)){var o=n.label,i=n.children,l=n.key,a=n.type,c=(0,u.Z)(n,eh),s=null!=l?l:"tmp-".concat(t);return i||"group"===a?"group"===a?v.createElement(ez,(0,r.Z)({key:s},c,{title:o}),e(i)):v.createElement(eR,(0,r.Z)({key:s},c,{title:o}),e(i)):"divider"===a?v.createElement(e_,(0,r.Z)({key:s},c)):v.createElement(ed,(0,r.Z)({key:s},c),o)}return null}).filter(function(e){return e})}(eE)),eg(e,eO)},[ew,eE]),ni=v.useState(!1),nl=(0,a.Z)(ni,2),na=nl[0],nu=nl[1],nc=v.useRef(),ns=(t=(0,d.Z)(eN,{value:eN}),p=(c=(0,a.Z)(t,2))[0],Z=c[1],v.useEffect(function(){U+=1;var e="".concat(X,"-").concat(U);Z("rc-menu-uuid-".concat(e))},[]),p),nf="rtl"===ex,nd=(0,d.Z)(eL,{value:eV,postState:function(e){return e||eO}}),np=(0,a.Z)(nd,2),nv=np[0],nm=np[1],ny=function(e){var n=arguments.length>1&&void 0!==arguments[1]&&arguments[1];function t(){nm(e),null==e3||e3(e)}n?(0,m.flushSync)(t):t()},nh=v.useState(nv),ng=(0,a.Z)(nh,2),nZ=ng[0],nb=ng[1],nC=v.useRef(!1),nE=v.useMemo(function(){return("inline"===eS||"vertical"===eS)&&eM?["vertical",eM]:[eS,!1]},[eS,eM]),nw=(0,a.Z)(nE,2),nx=nw[0],nN=nw[1],nP="inline"===nx,nS=v.useState(nx),nM=(0,a.Z)(nS,2),nk=nM[0],nI=nM[1],nR=v.useState(nN),nK=(0,a.Z)(nR,2),nO=nK[0],nA=nK[1];v.useEffect(function(){nI(nx),nA(nN),nC.current&&(nP?nm(nZ):ny(eO))},[nx,nN]);var nT=v.useState(0),nD=(0,a.Z)(nT,2),nL=nD[0],nz=nD[1],n_=nL>=no.length-1||"horizontal"!==nk||eI;v.useEffect(function(){nP&&nb(nv)},[nv]),v.useEffect(function(){return nC.current=!0,function(){nC.current=!1}},[]);var nV=(b=v.useState({}),C=(0,a.Z)(b,2)[1],E=(0,v.useRef)(new Map),N=(0,v.useRef)(new Map),P=v.useState([]),I=(S=(0,a.Z)(P,2))[0],K=S[1],Y=(0,v.useRef)(0),J=(0,v.useRef)(!1),Q=function(){J.current||C({})},$=(0,v.useCallback)(function(e,n){var t,r=B(n);N.current.set(r,e),E.current.set(e,r),Y.current+=1;var o=Y.current;t=function(){o===Y.current&&Q()},Promise.resolve().then(t)},[]),ee=(0,v.useCallback)(function(e,n){var t=B(n);N.current.delete(t),E.current.delete(e)},[]),en=(0,v.useCallback)(function(e){K(e)},[]),et=(0,v.useCallback)(function(e,n){var t=(E.current.get(e)||"").split(G);return n&&I.includes(t[0])&&t.unshift(H),t},[I]),er=(0,v.useCallback)(function(e,n){return e.some(function(e){return et(e,!0).includes(n)})},[et]),eo=(0,v.useCallback)(function(e){var n="".concat(E.current.get(e)).concat(G),t=new Set;return(0,l.Z)(N.current.keys()).forEach(function(e){e.startsWith(n)&&t.add(N.current.get(e))}),t},[]),v.useEffect(function(){return function(){J.current=!0}},[]),{registerPath:$,unregisterPath:ee,refreshOverflowKeys:en,isSubPathKey:er,getKeyPath:et,getKeys:function(){var e=(0,l.Z)(E.current.keys());return I.length&&e.push(H),e},getSubPathKeys:eo}),nF=nV.registerPath,nj=nV.unregisterPath,nW=nV.refreshOverflowKeys,nG=nV.isSubPathKey,nB=nV.getKeyPath,nH=nV.getKeys,nq=nV.getSubPathKeys,nX=v.useMemo(function(){return{registerPath:nF,unregisterPath:nj}},[nF,nj]),nU=v.useMemo(function(){return{isSubPathKey:nG}},[nG]);v.useEffect(function(){nW(n_?eO:no.slice(nL+1).map(function(e){return e.key}))},[nL,n_]);var nY=(0,d.Z)(eF||ej&&(null===(es=no[0])||void 0===es?void 0:es.key),{value:eF}),nJ=(0,a.Z)(nY,2),nQ=nJ[0],n$=nJ[1],n0=q(function(e){n$(e)}),n1=q(function(){n$(void 0)});(0,v.useImperativeHandle)(n,function(){return{list:nc.current,focus:function(e){var n,t,r,o,i=null!=nQ?nQ:null===(n=no.find(function(e){return!e.props.disabled}))||void 0===n?void 0:n.key;i&&(null===(t=nc.current)||void 0===t||null===(r=t.querySelector("li[data-menu-id='".concat(g(ns,i),"']")))||void 0===r||null===(o=r.focus)||void 0===o||o.call(r,e))}}});var n4=(0,d.Z)(eq||[],{value:eX,postState:function(e){return Array.isArray(e)?e:null==e?eO:[e]}}),n2=(0,a.Z)(n4,2),n6=n2[0],n8=n2[1],n7=function(e){if(eG){var n,t=e.key,r=n6.includes(t);n8(n=eH?r?n6.filter(function(e){return e!==t}):[].concat((0,l.Z)(n6),[t]):[t]);var o=(0,i.Z)((0,i.Z)({},e),{},{selectedKeys:n});r?null==eY||eY(o):null==eU||eU(o)}!eH&&nv.length&&"inline"!==nk&&ny(eO)},n9=q(function(e){null==e5||e5(el(e)),n7(e)}),n5=q(function(e,n){var t=nv.filter(function(n){return n!==e});if(n)t.push(e);else if("inline"!==nk){var r=nq(e);t=t.filter(function(e){return!r.has(e)})}(0,y.Z)(nv,t,!0)||ny(t,!0)}),n3=q(e9),te=(ei=function(e,n){var t=null!=n?n:!nv.includes(e);n5(e,t)},ea=v.useRef(),(eu=v.useRef()).current=nQ,ec=function(){R.Z.cancel(ea.current)},v.useEffect(function(){return function(){ec()}},[]),function(e){var n=e.which;if([].concat(F,[L,z,_,V]).includes(n)){var t=function(){return u=new Set,c=new Map,s=new Map,nH().forEach(function(e){var n=document.querySelector("[data-menu-id='".concat(g(ns,e),"']"));n&&(u.add(n),s.set(n,e),c.set(e,n))}),u};t();var r=function(e,n){for(var t=e||document.activeElement;t;){if(n.has(t))return t;t=t.parentElement}return null}(c.get(nQ),u),i=s.get(r),l=function(e,n,t,r){var i,l,a,u,c="prev",s="next",f="children",d="parent";if("inline"===e&&r===L)return{inlineTrigger:!0};var p=(i={},(0,o.Z)(i,T,c),(0,o.Z)(i,D,s),i),v=(l={},(0,o.Z)(l,O,t?s:c),(0,o.Z)(l,A,t?c:s),(0,o.Z)(l,D,f),(0,o.Z)(l,L,f),l),m=(a={},(0,o.Z)(a,T,c),(0,o.Z)(a,D,s),(0,o.Z)(a,L,f),(0,o.Z)(a,z,d),(0,o.Z)(a,O,t?f:d),(0,o.Z)(a,A,t?d:f),a);switch(null===(u=({inline:p,horizontal:v,vertical:m,inlineSub:p,horizontalSub:m,verticalSub:m})["".concat(e).concat(n?"":"Sub")])||void 0===u?void 0:u[r]){case c:return{offset:-1,sibling:!0};case s:return{offset:1,sibling:!0};case d:return{offset:-1,sibling:!1};case f:return{offset:1,sibling:!1};default:return null}}(nk,1===nB(i,!0).length,nf,n);if(!l&&n!==_&&n!==V)return;(F.includes(n)||[_,V].includes(n))&&e.preventDefault();var a=function(e){if(e){var n=e,t=e.querySelector("a");null!=t&&t.getAttribute("href")&&(n=t);var r=s.get(e);n$(r),ec(),ea.current=(0,R.Z)(function(){eu.current===r&&n.focus()})}};if([_,V].includes(n)||l.sibling||!r){var u,c,s,f,d=j(f=r&&"inline"!==nk?function(e){for(var n=e;n;){if(n.getAttribute("data-menu-list"))return n;n=n.parentElement}return null}(r):nc.current,u);a(n===_?d[0]:n===V?d[d.length-1]:W(f,u,r,l.offset))}else if(l.inlineTrigger)ei(i);else if(l.offset>0)ei(i,!0),ec(),ea.current=(0,R.Z)(function(){t();var e=r.getAttribute("aria-controls");a(W(document.getElementById(e),u))},5);else if(l.offset<0){var p=nB(i,!0),v=p[p.length-2],m=c.get(v);ei(v,!1),a(m)}}null==ne||ne(e)});v.useEffect(function(){nu(!0)},[]);var tn=v.useMemo(function(){return{_internalRenderMenuItem:nn,_internalRenderSubMenuItem:nt}},[nn,nt]),tt="horizontal"!==nk||eI?no:no.map(function(e,n){return v.createElement(w,{key:e.key,overflowDisabled:n>nL},e)}),tr=v.createElement(f.Z,(0,r.Z)({id:eN,ref:nc,prefixCls:"".concat(ev,"-overflow"),component:"ul",itemComponent:ed,className:s()(ev,"".concat(ev,"-root"),"".concat(ev,"-").concat(nk),eb,(ef={},(0,o.Z)(ef,"".concat(ev,"-inline-collapsed"),nO),(0,o.Z)(ef,"".concat(ev,"-rtl"),nf),ef),ey),dir:ex,style:eZ,role:"menu",tabIndex:void 0===eC?0:eC,data:tt,renderRawItem:function(e){return e},renderRawRest:function(e){var n=e.length,t=n?no.slice(-n):null;return v.createElement(eR,{eventKey:H,title:e8,disabled:n_,internalPopupClose:0===n,popupClassName:e7},t)},maxCount:"horizontal"!==nk||eI?f.Z.INVALIDATE:f.Z.RESPONSIVE,ssr:"full","data-menu-list":!0,onVisibleChange:function(e){nz(e)},onKeyDown:te},nr));return v.createElement(k.Provider,{value:tn},v.createElement(h.Provider,{value:ns},v.createElement(w,{prefixCls:ev,rootClassName:ey,mode:nk,openKeys:nv,rtl:nf,disabled:ek,motion:na?eQ:null,defaultMotions:na?e$:null,activeKey:nQ,onActive:n0,onInactive:n1,selectedKeys:n6,inlineIndent:void 0===eJ?24:eJ,subMenuOpenDelay:void 0===eA?.1:eA,subMenuCloseDelay:void 0===eT?.1:eT,forceSubMenuRender:eD,builtinPlacements:e1,triggerSubMenuAction:void 0===e0?"hover":e0,getPopupContainer:n3,itemIcon:e4,expandIcon:e2,onItemClick:n9,onOpenChange:n5},v.createElement(M.Provider,{value:nU},tr),v.createElement("div",{style:{display:"none"},"aria-hidden":!0},v.createElement(x.Provider,{value:nX},no)))))}),eT=["className","title","eventKey","children"],eD=["children"],eL=function(e){var n=e.className,t=e.title,o=(e.eventKey,e.children),i=(0,u.Z)(e,eT),l=v.useContext(E).prefixCls,a="".concat(l,"-item-group");return v.createElement("li",(0,r.Z)({role:"presentation"},i,{onClick:function(e){return e.stopPropagation()},className:s()(a,n)}),v.createElement("div",{role:"presentation",className:"".concat(a,"-title"),title:"string"==typeof t?t:void 0},t),v.createElement("ul",{role:"group",className:"".concat(a,"-list")},o))};function ez(e){var n=e.children,t=(0,u.Z)(e,eD),r=eg(n,S(t.eventKey));return N()?r:v.createElement(eL,(0,ee.Z)(t,["warnKey"]),r)}function e_(e){var n=e.className,t=e.style,r=v.useContext(E).prefixCls;return N()?null:v.createElement("li",{className:s()("".concat(r,"-item-divider"),n),style:t})}eA.Item=ed,eA.SubMenu=eR,eA.ItemGroup=ez,eA.Divider=e_;var eV=eA}}]);