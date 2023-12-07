import React, { useState } from 'react';
import { ListGroupItem, Input } from 'reactstrap';
import { FcFile } from "react-icons/fc";
import ReactMarkdown from 'react-markdown'

const Recommendation = ({ recommendation, level, isSelected, toggleSelectability }) => {

    const [isOpen, setIsOpen] = useState(false);

    if (!level) {
        level = 0;
    }

    const getPaddingLeft = () => {
        return level * 30;
    }

    const toggleIsOpen = (e) => {
        if (e.target.tagName === "INPUT") {
            return;
        }
        setIsOpen(!isOpen);
    }

    const toggleIsSelect = (category) => {
        toggleSelectability(category);
    }

    return (
        <>
            <ListGroupItem style={{ paddingLeft: getPaddingLeft() + 'px' }} action tag="button" onClick={toggleIsOpen}>
                <Input className="form-check-input me-2" type="checkbox" checked={isSelected(recommendation.id)} onChange={() => toggleIsSelect(recommendation)} /> <FcFile /> {recommendation.title}
            </ListGroupItem>
            {isOpen ? (
                <ListGroupItem style={{ paddingLeft: getPaddingLeft() + 'px' }} action tag="button" onClick={toggleIsOpen}>
                    <div className="ps-5">
                        <ReactMarkdown>{recommendation.description}</ReactMarkdown>
                    </div>
                </ListGroupItem>
            ) : null}
        </>
    )
}

export default Recommendation;