(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[7719],{93146:function(e,t,n){(window.__NEXT_P=window.__NEXT_P||[]).push(["/options/feedback",function(){return n(24034)}])},21826:function(e,t,n){"use strict";var r=n(1413),l=n(67294),a=n(96606),s=n(881),i=function(e,t){return l.createElement(s.Z,(0,r.Z)((0,r.Z)({},e),{},{ref:t,icon:a.Z}))};i.displayName="SearchOutlined",t.Z=l.forwardRef(i)},8123:function(e,t,n){"use strict";var r=n(85893),l=n(34223),a=n(66946);n(67294),t.Z=function(e){return(0,r.jsxs)("div",{className:"pt-5 pb-5",children:[(null==e?void 0:e.loading)&&(0,r.jsx)("div",{style:{width:"90%"},children:(0,r.jsx)(l.Z,{round:!0,active:!0})}),!(null==e?void 0:e.loading)&&(0,r.jsx)(a.Z,{description:!1,children:(0,r.jsx)("div",{className:"disable",children:"Kh\xf4ng c\xf3 dữ liệu"})})]})}},74471:function(e,t,n){"use strict";var r=n(85893),l=n(21826),a=n(96369),s=n(16655),i=n(74253),o=n(77889),c=n(51095),d=n(30381),u=n.n(d),h=n(67294);t.Z=function(){let e=arguments.length>0&&void 0!==arguments[0]?arguments[0]:null,t=arguments.length>1&&void 0!==arguments[1]?arguments[1]:null,n=arguments.length>2&&void 0!==arguments[2]?arguments[2]:null,d=arguments.length>3&&void 0!==arguments[3]?arguments[3]:"text",m=arguments.length>4&&void 0!==arguments[4]?arguments[4]:[];arguments.length>5&&void 0!==arguments[5]&&arguments[5];let[f,p]=(0,h.useState)(!1),[g,x]=(0,h.useState)(null),{RangePicker:v}=a.default,j=(0,h.useRef)(null),{Option:b}=s.default,y=e=>{x(e)},w=n=>{if(t&&null!=g){switch(d){case"text":case"select":t(n,e);break;case"date":t(n=u()(g.toDate()).format("YYYY/MM/DD"),e);break;case"date-range":t({fromDate:u()(g[0].toDate()).format("YYYY/MM/DD"),toDate:u()(g[1].toDate()).format("YYYY/MM/DD")},e)}p(!1)}},N=()=>{n&&(n(),y(null),p(!1))},k=()=>{let e;switch(d){case"text":e=(0,r.jsx)(i.default,{ref:j,value:g,placeholder:"T\xecm kiếm",onPressEnter:e=>w(g),onChange:e=>y(e.target.value),style:{marginBottom:8,display:"block"}});break;case"select":e=(0,r.jsx)(s.default,{ref:j,value:g,onChange:e=>y(e),style:{marginBottom:8,display:"block"},placeholder:"Lọc",optionFilterProp:"children",showSearch:!0,children:m.map((e,t)=>(0,r.jsx)(b,{value:e.value,children:e.title},t))});break;case"date":e=(0,r.jsx)(a.default,{style:{marginBottom:8,display:"block"},autoFocus:!0,format:"DD/MM/YYYY",onChange:(e,t)=>y(e)});break;case"date-range":e=(0,r.jsx)("div",{style:{marginBottom:8,display:"block"},children:(0,r.jsx)(v,{format:"DD/MM/YYYY",autoFocus:!0,ranges:{Today:[u()(),u()()],"This Month":[u()().startOf("month"),u()().endOf("month")]},onChange:(e,t)=>y(e)})})}return e};return(0,h.useEffect)(()=>{f&&setTimeout(()=>{var e,t;null===(t=j.current)||void 0===t||null===(e=t.select)||void 0===e||e.call(t)},100)},[f]),{filterDropdown:()=>(0,r.jsxs)("div",{style:{padding:8},children:[k(),(0,r.jsxs)(o.Z,{children:[(0,r.jsx)(c.Z,{type:"primary",onClick:()=>w(g),icon:(0,r.jsx)(l.Z,{}),size:"small",style:{width:90},children:"T\xecm"}),(0,r.jsx)(c.Z,{onClick:N,size:"small",style:{width:90},children:"Kh\xf4i phục"})]})]}),filterDropdownVisible:f,filterIcon:e=>(0,r.jsx)(l.Z,{}),onFilterDropdownVisibleChange:e=>{e?p(!0):p(!1)}}}},48895:function(e,t,n){"use strict";n.d(t,{Z:function(){return c}});var r=n(85893),l=n(96361),a=n(28210),s=n(67294),i=n(8123),o=e=>{let{record:t,visible:n,x:l,y:a,menuContext:i}=e,o=(0,s.useMemo)(()=>i.map(e=>{let t=e.components;return(0,r.jsxs)(r.Fragment,{children:[(0,r.jsx)(t,{}),e.text]})}),[i]);return n&&(0,r.jsx)("ul",{className:"context-menu-table",style:{left:"".concat(l,"px"),top:"".concat(a,"px")},children:o.map(e=>(0,r.jsx)("li",{className:"menu-context-item",children:e}))})},c=e=>{let t;let{columns:n,children:c,TitleCard:d,Extra:u,className:h,rowSelection:m,keySelection:f="",loading:p,bordered:g}=e,{total:x,current:v,expand:j,data:b,onExpand:y,onChangePage:w,menuContext:N,pageSize:k}=e,[O,C]=(0,s.useState)([]),[Z,D]=(0,s.useState)([{currentPage:1,listKeys:[]}]),[E,S]=(0,s.useState)(1),[R,I]=(0,s.useState)(null),[Y,F]=(0,s.useState)({record:null,visible:!1,x:0,y:0}),T=(e,t)=>{if(S(e),Z.some(t=>t.currentPage==e)||Z.push({currentPage:e,listKeys:[]}),D([...Z]),void 0===w)return e;w(e,t)};return(0,s.useEffect)(()=>{if(b){let e=JSON.parse(JSON.stringify(b));e.forEach((e,t)=>{let n=t.toString();f&&(n=(null==e?void 0:e[f])||"NOT_FOUND"),e.key=n}),C(e)}},[b]),(0,r.jsx)("div",{className:"wrap-table",children:(0,r.jsxs)(l.Z,{className:"".concat(h&&h),title:d,extra:u,children:[c,0==O.length&&(0,r.jsx)(i.Z,{loading:p}),O.length>0&&(0,r.jsxs)(r.Fragment,{children:[(0,r.jsx)(a.Z,{loading:p,bordered:g,rowSelection:m,scroll:{x:"max-content",y:window.innerHeight-295},columns:n,dataSource:O,size:"middle",pagination:{pageSize:k||30,pageSizeOptions:["30"],total:x&&x,current:v&&v,showTotal:()=>x&&(0,r.jsxs)("div",{className:"font-weight-black",children:["Tổng cộng: ",x]}),onChange:(e,t)=>T(e,t)},rowClassName:(e,t)=>t==R?"active":t%2==0?"row-light":"row-dark",onRow:(e,t)=>({onContextMenu:t=>{if(!N)return null;t.preventDefault(),Y.visible||document.addEventListener("click",function e(){F({...Y,visible:!1}),document.removeEventListener("click",e)}),F({...Y,record:e,visible:!0,x:t.clientX,y:t.clientY})},onClick:()=>I(t)}),expandable:Z[0].listKeys.length>0&&j,expandedRowRender:j?(e,t,n,r)=>r?j:null:void 0,onExpandedRowsChange:e=>{if(I(parseInt(e[e.length-1])),Z.some(e=>e.currentPage==E)){let t=Z.findIndex(e=>e.currentPage==E);Z[t].listKeys=e}D([...Z])},onExpand:(e,t)=>{void 0!==y&&y(t)},expandedRowKeys:(t=null,(t=Z.some(e=>e.currentPage==E)?Z.find(e=>e.currentPage===E).listKeys:[]).length>1&&t.splice(t.length-2,1),t)}),N&&(0,r.jsx)(o,{...Y,menuContext:N})]})]})})}},24034:function(e,t,n){"use strict";n.r(t),n.d(t,{default:function(){return F}});var r=n(85893),l=n(67294),a=n(26674),s=n(30381),i=n.n(s),o=n(59178);let c="/api/FeedbackCategorys",d={getAll:e=>o.e.get(c,{params:e}),update:e=>o.e.put(c,e,{}),add:e=>o.e.post(c,e)};var u=n(77024),h=n(92633),m=n(63237),f=n(58416);let p=l.memo(e=>{let[t,n]=(0,l.useState)(!1),{feedbackId:a,reloadData:s}=e,i=async()=>{try{var e;n(!1);let t=await d.update({ID:a,Enable:!1});(0,f.fr)("success",null===(e=t.data)||void 0===e?void 0:e.message),s()}catch(e){n(!1),(0,f.fr)("error",e.message)}};return(0,r.jsxs)(r.Fragment,{children:[(0,r.jsx)(u.Z,{title:"X\xf3a",children:(0,r.jsx)("button",{className:"btn btn-icon delete",onClick:()=>{n(!0)},children:(0,r.jsx)(h.Z,{})})}),(0,r.jsx)(m.Z,{title:"X\xf3a phản hồi",visible:t,onCancel:()=>n(!1),footer:(0,r.jsxs)(r.Fragment,{children:[(0,r.jsx)("button",{onClick:()=>n(!1),className:"btn btn-outline mr-2",children:"Hủy"}),(0,r.jsx)("button",{onClick:()=>i(),className:"btn btn-danger",children:"X\xf3a"})]}),children:(0,r.jsx)("p",{className:"text-base mb-4",children:"Bạn c\xf3 muốn x\xf3a phản hồi n\xe0y?"})})]})});var g=n(16655),x=n(31379),v=n(74253),j=n(69361),b=n(32655),y=n(87536),w=n(92088),N=n(88291);let k=l.memo(e=>{let{Option:t}=g.default,[n,a]=(0,l.useState)(!1),{feedbackId:s,reloadData:i,feedbackDetail:o,currentPage:c}=e,[h]=x.Z.useForm(),[p,k]=(0,l.useState)([]),[O,C]=(0,l.useState)(!1),{setValue:Z}=(0,y.cI)(),D=async()=>{try{let e=await N.ar.getRole(0);200===e.status&&k(e.data.data)}catch(e){console.log("Lỗi lấy th\xf4ng tin roles: ",e),(0,f.fr)("error",e.message)}};(0,l.useEffect)(()=>{D()},[]);let E=async e=>{if(C(!0),s)try{let t=await d.update({...e,Enable:!0,ID:s});i(c),S(null==t?void 0:t.data.message)}catch(e){(0,f.fr)("error",e.message),C(!1)}else try{let t=await d.add({...e,Enable:!0});S(null==t?void 0:t.data.message),i(1),h.resetFields()}catch(e){(0,f.fr)("error",e.message),C(!1)}},S=e=>{(0,f.fr)("success",e),C(!1),a(!1)};return(0,l.useEffect)(()=>{o&&h.setFieldsValue(o)},[n]),(0,r.jsxs)(r.Fragment,{children:[s?(0,r.jsx)("button",{className:"btn btn-icon edit",onClick:()=>{a(!0)},children:(0,r.jsx)(u.Z,{title:"Cập nhật",children:(0,r.jsx)(b.Z,{})})}):(0,r.jsxs)("button",{className:"btn btn-warning add-new",onClick:()=>{a(!0)},children:[(0,r.jsx)(w.OrI,{size:18,className:"mr-2"}),"Th\xeam mới"]}),(0,r.jsx)(m.Z,{title:(0,r.jsx)(r.Fragment,{children:s?"Cập nhật loại phản hồi":"Tạo loại phản hồi"}),visible:n,onCancel:()=>a(!1),footer:null,children:(0,r.jsx)("div",{className:"container-fluid",children:(0,r.jsxs)(x.Z,{form:h,layout:"vertical",onFinish:E,children:[(0,r.jsx)("div",{className:"row",children:(0,r.jsx)("div",{className:"col-12",children:(0,r.jsx)(x.Z.Item,{name:"Role",label:"Role",rules:[{required:!0,message:"Vui l\xf2ng điền đủ th\xf4ng tin!"}],children:(0,r.jsx)(g.default,{className:"w-100 style-input",placeholder:"Chọn role người tạo ...",children:p&&p.map(e=>(0,r.jsx)(t,{value:e.ID,children:e.name},e.ID))})})})}),(0,r.jsx)("div",{className:"row",children:(0,r.jsx)("div",{className:"col-12",children:(0,r.jsx)(x.Z.Item,{name:"Name",label:"Loại phản hồi",rules:[{required:!0,message:"Vui l\xf2ng điền đủ th\xf4ng tin!"}],children:(0,r.jsx)(v.default,{placeholder:"Nhập v\xe0o loại phản hồi...",className:"style-input",onChange:e=>Z("Name",e.target.value),allowClear:!0})})})}),(0,r.jsx)("div",{className:"row ",children:(0,r.jsx)("div",{className:"col-12",children:(0,r.jsxs)("button",{type:"submit",className:"btn btn-primary w-100",children:[(0,r.jsx)(w.tfk,{size:18,className:"mr-2"}),"Lưu",!0==O&&(0,r.jsx)(j.Z,{className:"loading-base"})]})})})]})})})]})});var O=n(36070),C=n(64811);let Z=[{id:1,RoleName:"Admin"},{id:2,RoleName:"Gi\xe1o vi\xean"},{id:3,RoleName:"Học vi\xean"}];var D=e=>{let[t,n]=(0,l.useState)(!1),{Option:a}=g.default,[s]=x.Z.useForm(),i=t=>{e._onFilter(t),n(!1)},o=(0,r.jsx)("div",{className:"wrap-filter small",children:(0,r.jsx)(x.Z,{layout:"vertical",onFinish:i,children:(0,r.jsxs)("div",{className:"row",children:[(0,r.jsx)("div",{className:"col-md-12",children:(0,r.jsx)(x.Z.Item,{label:"Role",children:(0,r.jsxs)(g.default,{className:"style-input",placeholder:"Chọn role",onChange:e=>s.setFieldValue("RoleID",e),allowClear:!0,children:[Z.map(e=>(0,r.jsx)(a,{value:e.id,children:e.RoleName},e.id)),(0,r.jsx)(a,{value:"disabled",disabled:!0,children:"Disabled"})]})})}),(0,r.jsx)("div",{className:"col-md-12",children:(0,r.jsx)(x.Z.Item,{className:"mb-0",children:(0,r.jsxs)("button",{className:"btn btn-primary",style:{marginRight:"10px"},onClick:i,children:[(0,r.jsx)(w.vU7,{size:18,className:"mr-1"}),"T\xecm kiếm"]})})})]})})});return(0,r.jsx)(r.Fragment,{children:(0,r.jsx)("div",{className:"wrap-filter-parent",children:(0,r.jsx)(O.Z,{visible:t,placement:"bottomRight",content:o,trigger:"click",overlayClassName:"filter-popover",onVisibleChange:()=>{n(!t)},children:(0,r.jsx)("button",{className:"btn btn-secondary light btn-filter",onClick:()=>{t?n(!1):n(!0)},children:(0,r.jsx)(C.Z,{})})})})})},E=n(48895),S=n(74471);let R=[{id:1,RoleName:"Admin"},{id:2,RoleName:"Gi\xe1o vi\xean"},{id:3,RoleName:"Học vi\xean"}];var I=()=>{let e=[{title:"Role",dataIndex:"Role",render:e=>{var t;return(0,r.jsx)("div",{style:{width:"150px"},children:null===(t=R.find(t=>t.id==e))||void 0===t?void 0:t.RoleName})}},{title:"Loại phản hồi",dataIndex:"Name",...(0,S.Z)("Name",e=>{n(1),o({...a,search:e})},()=>{n(1),o(a)},"text"),render:e=>(0,r.jsx)("p",{className:"font-weight-black",children:e})},{title:"Modified By",dataIndex:"ModifiedBy"},{title:"Modified Date",dataIndex:"ModifiedOn",render:e=>i()(e).format("DD/MM/YYYY")},{render:e=>(0,r.jsxs)(r.Fragment,{children:[(0,r.jsx)(k,{feedbackDetail:e,feedbackId:e.ID,reloadData:e=>{v(e)},currentPage:t}),(0,r.jsx)(p,{feedbackId:e.ID,reloadData:e=>{v(e)},currentPage:t})]})}],[t,n]=(0,l.useState)(1),a={pageSize:10,pageIndex:t,search:null,Role:null},[s,o]=(0,l.useState)(a),[c,u]=(0,l.useState)(null),[h,m]=(0,l.useState)([]),[g,x]=(0,l.useState)({type:"GET_ALL",status:!1}),v=e=>{x({type:"GET_ALL",status:!0}),(async()=>{try{let t=await d.getAll({...s,pageIndex:e});200==t.status&&m(t.data.data),204==t.status?(n(1),o(a)):u(t.data.totalRow)}catch(e){(0,f.fr)("error",e.message)}finally{x({type:"GET_ALL",status:!1})}})()};(0,l.useEffect)(()=>{v(t)},[s]);let j=e=>{o({...a,Role:e.RoleID})};return(0,r.jsx)(E.Z,{Extra:(0,r.jsx)(k,{reloadData:e=>{n(1),v(e)}}),data:h,columns:e,TitleCard:(0,r.jsx)("div",{className:"extra-table",children:(0,r.jsx)(D,{_onFilter:e=>j(e)})})})};let Y=()=>(0,r.jsx)(I,{});Y.Layout=a.C;var F=Y},32655:function(e,t,n){"use strict";var r=n(67294),l=n(45697),a=n.n(l);function s(){return(s=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)Object.prototype.hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e}).apply(this,arguments)}var i=(0,r.forwardRef)(function(e,t){var n=e.color,l=e.size,a=void 0===l?24:l,i=function(e,t){if(null==e)return{};var n,r,l=function(e,t){if(null==e)return{};var n,r,l={},a=Object.keys(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||(l[n]=e[n]);return l}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(r=0;r<a.length;r++)n=a[r],!(t.indexOf(n)>=0)&&Object.prototype.propertyIsEnumerable.call(e,n)&&(l[n]=e[n])}return l}(e,["color","size"]);return r.createElement("svg",s({ref:t,xmlns:"http://www.w3.org/2000/svg",width:a,height:a,viewBox:"0 0 24 24",fill:"none",stroke:void 0===n?"currentColor":n,strokeWidth:"2",strokeLinecap:"round",strokeLinejoin:"round"},i),r.createElement("path",{d:"M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"}),r.createElement("path",{d:"M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"}))});i.propTypes={color:a().string,size:a().oneOfType([a().string,a().number])},i.displayName="Edit",t.Z=i},64811:function(e,t,n){"use strict";var r=n(67294),l=n(45697),a=n.n(l);function s(){return(s=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)Object.prototype.hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e}).apply(this,arguments)}var i=(0,r.forwardRef)(function(e,t){var n=e.color,l=e.size,a=void 0===l?24:l,i=function(e,t){if(null==e)return{};var n,r,l=function(e,t){if(null==e)return{};var n,r,l={},a=Object.keys(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||(l[n]=e[n]);return l}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(r=0;r<a.length;r++)n=a[r],!(t.indexOf(n)>=0)&&Object.prototype.propertyIsEnumerable.call(e,n)&&(l[n]=e[n])}return l}(e,["color","size"]);return r.createElement("svg",s({ref:t,xmlns:"http://www.w3.org/2000/svg",width:a,height:a,viewBox:"0 0 24 24",fill:"none",stroke:void 0===n?"currentColor":n,strokeWidth:"2",strokeLinecap:"round",strokeLinejoin:"round"},i),r.createElement("polygon",{points:"22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"}))});i.propTypes={color:a().string,size:a().oneOfType([a().string,a().number])},i.displayName="Filter",t.Z=i},92633:function(e,t,n){"use strict";var r=n(67294),l=n(45697),a=n.n(l);function s(){return(s=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)Object.prototype.hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e}).apply(this,arguments)}var i=(0,r.forwardRef)(function(e,t){var n=e.color,l=e.size,a=void 0===l?24:l,i=function(e,t){if(null==e)return{};var n,r,l=function(e,t){if(null==e)return{};var n,r,l={},a=Object.keys(e);for(r=0;r<a.length;r++)n=a[r],t.indexOf(n)>=0||(l[n]=e[n]);return l}(e,t);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);for(r=0;r<a.length;r++)n=a[r],!(t.indexOf(n)>=0)&&Object.prototype.propertyIsEnumerable.call(e,n)&&(l[n]=e[n])}return l}(e,["color","size"]);return r.createElement("svg",s({ref:t,xmlns:"http://www.w3.org/2000/svg",width:a,height:a,viewBox:"0 0 24 24",fill:"none",stroke:void 0===n?"currentColor":n,strokeWidth:"2",strokeLinecap:"round",strokeLinejoin:"round"},i),r.createElement("polyline",{points:"3 6 5 6 21 6"}),r.createElement("path",{d:"M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"}))});i.propTypes={color:a().string,size:a().oneOfType([a().string,a().number])},i.displayName="Trash",t.Z=i}},function(e){e.O(0,[6130,4838,7909,5970,8460,9915,6565,653,6655,1607,6361,5009,4253,9872,1379,861,8675,8210,6369,3732,6954,2888,9774,179],function(){return e(e.s=93146)}),_N_E=e.O()}]);