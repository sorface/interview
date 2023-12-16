import { FunctionComponent } from 'react';

import './Tabs.css';

export interface Tab {
  id: string;
  caption: string;
}

interface TabsProps {
  tabs: Tab[];
  activeTabId: Tab['id'];
  onTabClick: (tabId: Tab['id']) => void;
}

export const Tabs: FunctionComponent<TabsProps> = ({
  tabs,
  activeTabId,
  onTabClick,
}) => {
  return (
    <div className='tabs'>
      {tabs.map(tab => (
        <div
          key={tab.id}
          role='tab'
          className={`tabs-tab ${tab.id === activeTabId ? 'tabs-tab-active' : ''}`}
          onClick={() => onTabClick(tab.id)}
        >
          {tab.caption}
        </div>
      ))}
    </div>
  )
}
