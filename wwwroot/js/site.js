(()=>{const $=s=>document.querySelector(s);
const p=location.pathname.replace(/\/$/,'')||'/';
document.querySelectorAll('nav a:not(.btn)').forEach(a=>{const x=a.getAttribute('href').split('#')[0]||'/';a.classList.toggle('on',x===p||(x!=='/'&&p.toLowerCase().startsWith(x.toLowerCase())))});
$('#yr').textContent=new Date().getFullYear();
$('#burger').onclick=()=>{const o=$('#nav').classList.toggle('open');$('#burger').setAttribute('aria-expanded',o)};
const m=$('#modal'),close=()=>{m.hidden=true;try{sessionStorage.setItem('hr_seen','1')}catch(e){}};
let seen=false;try{seen=sessionStorage.getItem('hr_seen')}catch(e){}
if(!seen)setTimeout(()=>{if(m.hidden){m.hidden=false;$('#mx').focus()}},6000);
$('#mx').onclick=$('#later').onclick=close;m.onclick=e=>{if(e.target===m)close()};
addEventListener('keydown',e=>{if(e.key==='Escape'&&!m.hidden)close()});
$('#claim').onclick=()=>{try{localStorage.setItem('hr_promo','1')}catch(e){}close()};})();