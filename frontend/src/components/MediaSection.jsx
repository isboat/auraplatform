import { ArrowRight } from 'lucide-react';
import MediaCard from './MediaCard';
export default function MediaSection({title,kicker,items}){return <section className="media-section"><div className="section-heading"><div><span className="eyebrow">{kicker}</span><h2>{title}</h2></div><a href="/search">View all <ArrowRight size={16}/></a></div><div className="media-grid">{items.map(item=><MediaCard key={item.id} item={item}/>)}</div></section>}
