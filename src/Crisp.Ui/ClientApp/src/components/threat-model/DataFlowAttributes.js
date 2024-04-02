import React, { useState } from 'react';
import { Button, Label, Input, Row, Col, Tooltip } from 'reactstrap';
import { FiPlus, FiInfo } from "react-icons/fi";
import './DataFlowAttributes.css';

const DataFlowAttributes = ({ dataflowAttributes, setDataflowAttributes }) => {

    const [dataClassificationTooltipOpen, setDataClassificationTooltipOpen] = useState(false);

    const addDataflowAttribute = () => {
        const nextIndex = dataflowAttributes.length > 0
            ? Math.max(...dataflowAttributes.map(a => parseInt(a.number, 10))) + 1
            : 1;
        
        const newAttribute = {
            number: nextIndex.toString(),
            transport: 'HTTPS/TLS 1.2',
            dataClassification: 'Confidential',
            authentication: 'Microsoft Entra ID',
            authorization: 'RBAC',
            notes: ''
        };
        setDataflowAttributes(prev => [...prev, newAttribute]);
    }

    const deleteDataflowAttribute = (index) => {
        setDataflowAttributes(prev => prev.filter((_, attrIndex) => attrIndex !== index));
    }

    const onDataflowAttributeChange = (e, index) => {
        const { name, value } = e.target;
        setDataflowAttributes(prev =>
            prev.map((attribute, attrIndex) =>
                attrIndex === index ? { ...attribute, [name]: value } : attribute
            )
        );
    }

    const onDataClassificationTooltipToggle = () => setDataClassificationTooltipOpen(!dataClassificationTooltipOpen);

    return (
        <div>
            <Button color="success" onClick={addDataflowAttribute}><FiPlus /> Add attribute</Button>
            <Row className="mt-3">
                <Col md={1} className="mb-0">
                    <Label>#</Label>
                </Col>
                <Col className="ps-0 mb-0">
                    <Label>Transport Protocol</Label>
                </Col>
                <Col className="mb-0">
                    <Label>Data Classification <FiInfo id="data-classification-info" /></Label>
                </Col>
                <Col className="mb-0">
                    <Label>Authentication</Label>
                </Col>
                <Col className="mb-0">
                    <Label>Authorization</Label>
                </Col>
                <Col md={5} className="mb-0">
                    <Label>Notes</Label>
                </Col>
            </Row>
            {dataflowAttributes.map((a, index) => (
                <Row key={index} className="mb-1">
                    <Col md={1}>
                        <Input name="number" value={a.number} onChange={(e) => onDataflowAttributeChange(e, index)} />
                    </Col>
                    <Col className="ps-0">
                        <Input name="transport" value={a.transport} onChange={(e) => onDataflowAttributeChange(e, index)} />
                    </Col>
                    <Col>
                        <Input type="select" name="dataClassification" value={a.dataClassification} onChange={(e) => onDataflowAttributeChange(e, index)}>
                            <option>Sensitive</option>
                            <option>Confidential</option>
                            <option>Private</option>
                            <option>Proprietary</option>
                            <option>Public</option>
                        </Input>
                    </Col>
                    <Col>
                        <Input name="authentication" value={a.authentication} onChange={(e) => onDataflowAttributeChange(e, index)} />
                    </Col>
                    <Col>
                        <Input name="authorization" value={a.authorization} onChange={(e) => onDataflowAttributeChange(e, index)} />
                    </Col>
                    <Col md={5}>
                        <Row>
                            <Col md={11}>
                                <Input name="notes" type="textarea" value={a.notes} onChange={(e) => onDataflowAttributeChange(e, index)} />
                            </Col>
                            <Col md={1} className="ps-0">
                                <Button color="danger" outline onClick={() => deleteDataflowAttribute(index)}>X</Button>
                            </Col>
                        </Row>
                    </Col>
                </Row>
            ))
            }
            <Tooltip isOpen={dataClassificationTooltipOpen} target="data-classification-info" toggle={onDataClassificationTooltipToggle}>
                <ul>
                    <li><b>Sensitive</b><br/>Data that is to have the most limited access and requires a high degree of integrity. This is typically data that will do the most damage to the organization should it be disclosed. Personal data (including PII) falls into this category and includes any identifier, such as name, an identification number, location data, online identifier. This also includes data related to one or more factors specific to the physical, psychological, genetic, mental, economic, cultural, or social identity of an individual.</li>
                    <li><b>Confidential</b><br/>Data that might be less restrictive within the company but might cause damage if disclosed.</li>
                    <li><b>Private</b><br/>Private data is usually compartmental data that might not do the company damage but must be kept private for other reasons. Human resources data is one example of data that can be classified as private.</li>
                    <li><b>Proprietary</b><br/>Proprietary data is data that is disclosed outside the company on a limited basis or contains information that could reduce the company's competitive advantage, such as the technical specifications of a new product.</li>
                    <li><b>Public</b><br/>Public data is the least sensitive data used by the company and would cause the least harm if disclosed. This could be anything from data used for marketing to the number of employees in the company.</li>
                </ul>
            </Tooltip>
        </div>
    );
};

export default DataFlowAttributes;
