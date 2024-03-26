import React, { useState, useCallback, useEffect } from 'react';
import { ListGroupItem, Input } from 'reactstrap';
import { FcFile } from "react-icons/fc";
import ReactMarkdown from 'react-markdown';

const Recommendation = ({ recommendation, level = 0, isSelected, toggleSelectability, isOpen = false }) => {
    const [isOpenLocal, setIsOpenLocal] = useState(isOpen);

    useEffect(() => {
        setIsOpenLocal(isOpen);
    }, [isOpen]);

    const toggleIsOpen = useCallback((e) => {
        if (e.target.tagName !== "INPUT") {
            setIsOpenLocal(!isOpenLocal);
        }
    }, [isOpenLocal]);

    const toggleIsSelect = useCallback(() => {
        toggleSelectability(recommendation);
    }, [recommendation, toggleSelectability]);

    const paddingLeft = level * 30;

    return (
        <>
            <ListGroupItem style={{ paddingLeft: `${paddingLeft}px` }} action tag="button" onClick={toggleIsOpen}>
                <Input className="form-check-input me-2" type="checkbox" checked={isSelected(recommendation.id)} onChange={toggleIsSelect} /> <FcFile /> {recommendation.title}
            </ListGroupItem>
            {isOpenLocal && (
                <ListGroupItem style={{ paddingLeft: `${paddingLeft}px` }} action tag="button" onClick={toggleIsOpen}>
                    <div className="ps-5">
                        <ReactMarkdown>{recommendation.description}</ReactMarkdown>
                    </div>
                </ListGroupItem>
            )}
        </>
    );
};

export default Recommendation;
