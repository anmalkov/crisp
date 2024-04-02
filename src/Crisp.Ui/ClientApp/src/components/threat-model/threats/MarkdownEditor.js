import React, { useState } from 'react';
import { FormGroup, Label, Input, Nav, NavItem, NavLink, TabContent, TabPane } from 'reactstrap';
import ReactMarkdown from 'react-markdown';

const MarkdownEditor = ({ label, fieldName, placeholder, value, setValue }) => {
    const [activeTab, setActiveTab] = useState('1');

    const toggleTab = (tab) => {
        if (activeTab !== tab) setActiveTab(tab);
    };

    const onDescriptionChange = (e) => {
        setValue(e.target.value);
    };

    return (
        <FormGroup>
            <Label for={fieldName} >{label}</Label>
            <Nav pills>
                <NavItem>
                    <NavLink className={activeTab === '1' ? 'active' : ''} onClick={() => { toggleTab('1'); }}>
                        Markdown
                    </NavLink>
                </NavItem>
                <NavItem>
                    <NavLink className={activeTab === '2' ? 'active' : ''} onClick={() => { toggleTab('2'); }}>
                        Preview
                    </NavLink>
                </NavItem>
            </Nav>
            <TabContent activeTab={activeTab} className="mt-2">
                <TabPane tabId="1">
                    <Input
                        id={fieldName}
                        name={fieldName}
                        type="textarea"
                        placeholder={placeholder}
                        value={value}
                        onChange={onDescriptionChange}
                        rows="20"
                    />
                </TabPane>
                <TabPane tabId="2">
                    <ReactMarkdown>{value}</ReactMarkdown>
                </TabPane>
            </TabContent>
        </FormGroup>
    );
};

export default MarkdownEditor;
