(()=>{const $=s=>document.querySelector(s);let pick=null;
const promo=()=>{try{return localStorage.getItem('hr_promo')==='1'}catch(e){return false}};
async function load(){pick=null;const box=$('#slots');box.innerHTML='';const d=$('#dt').value;
 if(!d){$('#sn').textContent='Choose a date to see available times.';return}
 const r=await fetch(`/Booking/Slots?date=${d}&service=${$('#svc').value}&barber=${$('#brb').value}`);
 if(!r.ok){$('#sn').textContent='Could not load times. Please try again.';return}
 const j=await r.json();
 if(j.closed){$('#sn').textContent='We are closed on Sundays. Please pick another day.';return}
 let n=0;j.slots.forEach(s=>{const b=document.createElement('button');b.type='button';b.textContent=s.label;b.disabled=!s.ok;if(s.ok)n++;
  b.onclick=()=>{pick=s.t;box.querySelectorAll('button').forEach(x=>x.classList.remove('sel'));b.classList.add('sel')};box.appendChild(b)});
 $('#sn').textContent=n?'Greyed-out times are already taken.':'No times left on this day. Try another date.'}
['#dt','#svc','#brb'].forEach(s=>$(s).addEventListener('change',load));
$('#f').addEventListener('submit',async e=>{e.preventDefault();const er=$('#err');er.textContent='';
 if(pick===null){er.textContent=$('#dt').value?'Please choose a time.':'Please choose a date.';return}
 const btn=$('#go');btn.disabled=true;
 try{const r=await fetch('/Booking/Create',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({serviceId:$('#svc').value,barberId:$('#brb').value,date:$('#dt').value,startMin:pick,name:$('#nm').value,phone:$('#ph').value,email:$('#em').value,promo:promo()})});
  const j=await r.json();if(r.ok)location.href=j.url;else{er.textContent=j.error||'Something went wrong.';if(r.status===409)load()}}
 catch(x){er.textContent='Network problem. Please try again.'}btn.disabled=false});
load();})();