"use strict";(self.webpackChunk_N_E=self.webpackChunk_N_E||[]).push([[6758],{45201:function(e,t,a){a.r(t);var n=a(85893),s=a(69361),r=a(77024),i=a(46556),c=a(51095),l=a(67294),o=a(78268),d=a(27104),h=a(89865),u=a(79930),m=a(32399);let x="",g=0;t.default=e=>{let{setLinkRecord:t,linkRecord:a,isHideBar:p,onDelete:j,disabled:f}=e,[v,y]=(0,l.useState)(!1),[I,b]=(0,l.useState)(!1),[Z,N]=(0,l.useState)("00:00:00"),S=()=>{var e,t,a;g+=1,N((t=Math.floor(parseInt(e=parseInt(g,10).toString())/3600).toString(),a=Math.floor((parseInt(e)-3600*parseInt(t))/60).toString(),e=(parseInt(e)-3600*parseInt(t)-60*parseInt(a)).toString(),10>parseInt(t)&&(t="0"+t),10>parseInt(a)&&(a="0"+a),10>parseInt(e)&&(e="0"+e),t+":"+a+":"+e))},[k,w]=(0,l.useState)("");(0,l.useEffect)(()=>{navigator.mediaDevices&&navigator.mediaDevices.getUserMedia?navigator.mediaDevices.getUserMedia({audio:!0}).then(e=>{w(new MediaRecorder(e))}).catch(function(e){console.log("Kh\xf4ng thể lấy được quyền ghi \xe2m: "+e)}):console.log("Tr\xecnh duyệt của bạn kh\xf4ng hỗ trợ ghi \xe2m!")},[]);let D=async e=>{var a,n;clearInterval(x),g=0,N("00:00:00");let s=(a=e.blob,n="record-audio.mp3",a.lastModifiedDate=new Date,a.name=n,new File([a],n,{lastModified:new Date().getTime(),type:a.type}));y(!0),b(!1);try{let e=await u.X.uploadImage(s);200==e.status&&t(e.data.data)}catch(e){}finally{y(!1)}};async function M(){m.ZP.stop(e=>{console.log("event: ",e),D(e)})}return(0,n.jsx)("div",{children:(0,n.jsxs)("div",{id:"recoder-".concat(e.id),style:{display:"flex",flexDirection:"row",alignItems:"center"},children:[v?(0,n.jsx)("div",{className:"recoder",children:(0,n.jsx)(s.Z,{className:"mr-3"})}):!I&&a&&(0,n.jsxs)(n.Fragment,{children:[!!j&&!f&&(0,n.jsx)(r.Z,{title:"Xo\xe1 \xe2m thanh",children:(0,n.jsx)(i.Z,{okText:"X\xf3a",cancelText:"Hủy",onConfirm:()=>!!j&&j(),title:"Bạn muốn x\xf3a bản ghi n\xe0y?",children:(0,n.jsx)(c.Z,{className:"btn-icon mr-2",style:{borderWidth:0},children:(0,n.jsx)(o.Z,{color:"red"})})})}),(0,n.jsx)("audio",{id:"audio-".concat(e.id),className:"mr-3 custom-audio-recorder-netxt",controls:!0,children:(0,n.jsx)("source",{src:a,type:"audio/mpeg"})})]}),!I&&!f&&(0,n.jsx)(r.Z,{title:a?"Ghi lại":"Bắt đầu ghi \xe2m",children:(0,n.jsx)("button",{className:"btn-record-next start",onClick:function(){try{m.ZP.start(),b(!0),x=setInterval(S,1e3)}catch(e){console.log("Can not start recoder: ",e)}},children:(0,n.jsx)(d.Z,{})})}),I&&!f&&(0,n.jsxs)(n.Fragment,{children:[(0,n.jsx)(r.Z,{title:"Lưu lại",children:(0,n.jsx)("button",{className:"btn-record save",onClick:M,children:(0,n.jsx)(h.Z,{})})}),(0,n.jsx)("div",{className:"ml-3",style:{fontSize:16},children:Z})]})]})})}}}]);