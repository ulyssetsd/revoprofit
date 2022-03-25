import React, { FC } from "react";

interface ITitleProps {
    items: Array<{
        title: string;
        subtitle: string;
    }>
}

const Accordion: FC<ITitleProps> = ({ items }) => {
    return (
        <div className="accordion mt-5" id="accordionPanelsStayOpenExample">
            {items.map(({ title, subtitle }, index) => {
                return (
                    <div className="accordion-item" key={index}>
                        <h2 className="accordion-header" id={`panelsStayOpen-heading${index}`} >
                            <button
                                className="accordion-button collapsed"
                                type="button"
                                data-bs-toggle="collapse"
                                data-bs-target={`#panelsStayOpen-collapse${index}`}
                                aria-expanded="false"
                                aria-controls={`#panelsStayOpen-collapse${index}`}>
                                {title}
                            </button>
                        </h2>
                        <div id={`panelsStayOpen-collapse${index}`} className="accordion-collapse collapse" aria-labelledby={`panelsStayOpen-heading${index}`}>
                            <div className="accordion-body">
                                {subtitle}
                            </div>
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

export default Accordion;