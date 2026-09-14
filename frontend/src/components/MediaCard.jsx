import { Eye, Heart, Image, Music2, Play } from 'lucide-react';
const icons={video:Play,image:Image,audio:Music2};
// Shared with metric-heavy pages to keep number formatting consistent.
// eslint-disable-next-line react-refresh/only-export-components
export const compact=(value)=>Intl.NumberFormat('en',{notation:'compact',maximumFractionDigits:1}).format(value);
export default function MediaCard({item}) { const Icon=icons[item.type]; return <a className="media-card" href={`/media/${item.id}`}><div className={`media-art ${item.color}`}><span className="type-pill"><Icon size={13}/>{item.type}</span>{item.duration&&<span className="duration">{item.duration}</span>}<span className="play"><Icon size={25}/></span></div><div className="card-body"><span className="eyebrow">{item.category}</span><h3>{item.title}</h3><p>by {item.ownerName}</p><div className="stats"><span><Eye size={14}/>{compact(item.views)}</span><span><Heart size={14}/>{compact(item.likes)}</span></div></div></a> }
